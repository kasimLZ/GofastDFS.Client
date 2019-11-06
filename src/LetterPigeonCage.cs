using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GoFastDFS.Client.Options;
using GoFastDFS.Client.Service;
using Newtonsoft.Json;

namespace GoFastDFS.Client
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class LetterPigeonCage: ILetterPigeonCage
	{
		/// <summary>
		/// Regular expression for the file path address returned by the DFS server
		/// </summary>
		private const string FILE_PATH_URL_REGEX = @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$";

		/// <summary>
		/// Send file task queue
		/// </summary>
		private readonly EnvelopeQueue Envelopes = new EnvelopeQueue();

		/// <summary>
		/// Management set of currently active sending task threads
		/// </summary>
		private readonly HashSet<Task> LetterPigeons = new HashSet<Task>();

		/// <summary>
		/// Uploading machine's workbench index
		/// </summary>
		private readonly ConcurrentDictionary<int, Workbench> Workbenchs = new ConcurrentDictionary<int, Workbench>();

		/// <summary>
		/// The currently active upload thread activator
		/// </summary>
		private Task PigeonRouser;

		/// <summary>
		/// Upload monitor's cancel controller
		/// </summary>
		private CancellationTokenSource Observer;

		/// <summary>
		/// Upload monitor's locker
		/// </summary>
		private readonly object ObserverLocker = new object();

		[Inject]
		private readonly IAuthoritionTokenService iAuthoritionTokenService;

		[Inject]
		private readonly IHttpClientFactory iHttpClientFactory;

		public LetterPigeonCage() => Envelopes.EnterNewEnvelopCallback += new EnterNewEnvelopHandler(ActivationPigeons);

		/// <summary>
		/// Add a single file upload task to the upload queue
		/// </summary>
		/// <param name="name">The name of the uploaded file, you need to keep the file after the format suffix name</param>
		/// <param name="file">File stream of uploaded files</param>
		/// <param name="options">Target endpoint setting item</param>
		/// <param name="immediate">Whether to join the priority queue</param>
		/// <returns>Upload receipt of file results</returns>
		public async Task<DeliverStatus> Dispatch(string name, Stream file, FastDfsEndPointOptions options, bool immediate = false)
		{
			var envelope = new Envelope { FileName = name, Options = options, Cargo = file };
			Envelopes.Enqueue(envelope, immediate);
			return await Task.Run(() => envelope.Status.Value);
		}

		/// <summary>
		/// Add upload tasks to the task queue in batches
		/// </summary>
		/// <param name="files">File name-stream key-value structure set, you can directly use the implementation type of <see cref="IDictionary{String, Stream}"/></param>
		/// <param name="options">Target endpoint setting item</param>
		/// <param name="immediate">Whether to join the priority queue</param>
		/// <returns>Collection of upload file result receipts</returns>
		public async Task<DeliverStatusCollection> Dispatch(ICollection<KeyValuePair<string, Stream>> files, FastDfsEndPointOptions options, bool immediate = false)
		{
			List<Envelope> EnvelopeGroup = new List<Envelope>(files.Count);
			foreach (var item in files)
			{
				var envelope = new Envelope { FileName = item.Key, Options = options, Cargo = item.Value };
				EnvelopeGroup.Add(envelope);
				Envelopes.Enqueue(envelope, immediate);
			}
			return await Task.Run(() => new DeliverStatusCollection(EnvelopeGroup.Select(a => a.Status.Value).ToList()));
		}

		/// <summary>
		/// Method of handling a single task
		/// </summary>
		/// <param name="envelope">Upload task object</param>
		/// <returns>Upload receipt of file results</returns>
		private async Task<DeliverStatus> Dispatch(Envelope envelope)
		{
			var deliver = new DeliverStatus { FileName = envelope.FileName };
			try
			{
				HttpContent content = GenerateHttpContent(envelope.FileName, envelope.Cargo, envelope.Options);

				HttpResponseMessage Response = await iHttpClientFactory.SendAsync(envelope.Options.EndPoint, content, HttpMethod.Post);

				deliver.Success = Response.IsSuccessStatusCode;
				deliver.RawResponse = await Response.Content.ReadAsStringAsync();

				if (deliver.Success)
				{
					//Currently GoFastdfs only supports json and string modes, which are processed directly here.
					switch (envelope.Options.Output)
					{
						case FastDfsOutput.Json:
							deliver.PayLoad = JsonConvert.DeserializeObject<PayloadInfo>(deliver.RawResponse); break;
						case FastDfsOutput.Text:
							if (Regex.IsMatch(deliver.RawResponse, FILE_PATH_URL_REGEX))
								deliver.PayLoad = new PayloadInfo { Url = deliver.RawResponse };
							else
								throw new InvalidDataException("Unrecognized URL path");
							break;
					}
				}
				else
				{
					throw new HttpRequestException($"Upload file failed, response status {Response.StatusCode.ToString()}");
				}
			}

			catch (Exception e)
			{
				deliver.Success = false;
				deliver.ErrorMessage = e;
			}
			return deliver;
		}

		/// <summary>
		/// Create a context to send a file request
		/// </summary>
		/// <param name="name">The name of the uploaded file, you need to keep the file after the format suffix name</param>
		/// <param name="file">File stream of uploaded files</param>
		/// <param name="options">Target endpoint setting item</param>
		/// <returns></returns>
		private MultipartFormDataContent GenerateHttpContent(string name, Stream file, FastDfsEndPointOptions options)
		{
			var Content = new MultipartFormDataContent {
				{ new StringContent(options.Output.StringValue()),"output" },
				{ new StringContent(options.FilePath ?? string.Empty), "path" },
				{ new StringContent(options.Scene ?? string.Empty), "scene" },
			};

			//If an authentication component exists, set the authentication field from the authentication component in the context
			if (options.AuthoritionComponent != null)
				options.AuthoritionComponent.AuthorizeFormSetting(Content, iAuthoritionTokenService.GetToken(options.AuthoritionComponent));

			Content.Add(new StreamContent(file), "file", name);

			return Content;
		}

		/// <summary>
		/// Try to start a new activator to try to start more upload threads
		/// </summary>
		private void ActivationPigeons()
		{
			//If there is already an activator thread currently working, then no new activator is created
			if (PigeonRouser != null && !PigeonRouser.IsCompleted) return;

			//Start a new activator thread
			PigeonRouser = Task.Run(() => { while (TryActivationPigeon()) ; });
		}

		/// <summary>
		/// Try to start a new uploader
		/// </summary>
		/// <param name="InheritWorkbench">继承ID</param>
		/// <returns>Whether to successfully launch an uploader instance</returns>
		private bool TryActivationPigeon(Workbench? InheritWorkbench = null)
		{
			lock (LetterPigeons)
			{
				//If the current number of upload threads is less than the maximum thread pool capacity, 
				//or less than the remaining number of tasks, then the start attempts to start a new thread
				if (LetterPigeons.Count < DefaultOptionManagement.MaxConnetPool && LetterPigeons.Count < Envelopes.Count)
				{
					Workbench workbench = InheritWorkbench ?? new Workbench();

					var letterPigeon = Task.Run(async () =>
					{
						if (workbench.envelope == null)
						{
							workbench.envelope = Envelopes.Dequeue();
							workbench.RetryTime = 0;
						}
							

						while (workbench.envelope != null)
						{
							if(workbench.RetryTime < DefaultOptionManagement.MaximumNumberOfRetries)
								workbench.envelope.Status = await Dispatch(workbench.envelope);
							else
								workbench.envelope.Status = new DeliverStatus { Success = false, ErrorMessage = new Exception("Exceeded the maximum number of retries") };
							workbench.envelope = Envelopes.Dequeue();
							workbench.RetryTime = 0;
						}
					});

					LetterPigeons.Add(letterPigeon);
					Workbenchs.TryAdd(letterPigeon.Id, workbench);

					RestObserver();

					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Reset upload thread watcher
		/// </summary>
		private void RestObserver()
		{
			lock (ObserverLocker)
			{
				//If it is due to create a new thread that needs to be reset,
				//need to cancel the currently working monitor.
				if (Observer != null) Observer.Cancel();

				//If there are no new threads in the thread pool, you do not need to start the monitor
				if (LetterPigeons.Count == 0) return; 

				Observer = new CancellationTokenSource();

				//Start a new monitor
				Task.WhenAny(LetterPigeons).ContinueWith(task =>
				{
					var DeadTask = task.Result;
					//Remove the stopped thread from the thread pool, and take out the workbench corresponding to the thread
					LetterPigeons.Remove(DeadTask);
					Workbenchs.TryRemove(DeadTask.Id, out Workbench workbench);

					//If the thread is naturally terminated, reset the monitor directly, 
					//otherwise try to start a new thread and inherit the workbench to continue the current task
					if (DeadTask.IsFaulted)
					{
						workbench.RetryTime++;
						TryActivationPigeon(workbench);
					}
					else
					{
						RestObserver();
					}
						
				}, Observer.Token);
			}
		}

		/// <summary>
		/// Uploader thread workbench
		/// </summary>
		private struct Workbench
		{
			/// <summary>
			/// number of retries
			/// </summary>
			internal int RetryTime;

			/// <summary>
			/// Upload task related content
			/// </summary>
			internal Envelope envelope;
		}
	}

	/// <summary>
	/// The collector of the sending task and the canonical interface of the manager that sent the task
	/// </summary>
	internal interface ILetterPigeonCage
	{
		/// <summary>
		/// Add a single file upload task to the upload queue
		/// </summary>
		/// <param name="name">The name of the uploaded file, you need to keep the file after the format suffix name</param>
		/// <param name="file">File stream of uploaded files</param>
		/// <param name="options">Target endpoint setting item</param>
		/// <param name="immediate">Whether to join the priority queue</param>
		/// <returns>Upload receipt of file results</returns>
		Task<DeliverStatus> Dispatch(string name, Stream file, FastDfsEndPointOptions options, bool immediate = false);

		/// <summary>
		/// Add upload tasks to the task queue in batches
		/// </summary>
		/// <param name="files">File name-stream key-value structure set, you can directly use the implementation type of <see cref="IDictionary{String, Stream}"/></param>
		/// <param name="options">Target endpoint setting item</param>
		/// <param name="immediate">Whether to join the priority queue</param>
		/// <returns>Collection of upload file result receipts</returns>
		Task<DeliverStatusCollection> Dispatch(ICollection<KeyValuePair<string, Stream>> files, FastDfsEndPointOptions options, bool immediate = false);
	}

}
