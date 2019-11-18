using GoFastDFS.Client.Options;
using GoFastDFS.Client.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GoFastDFS.Client
{
	/// <summary>
	/// Provide basic GoFastDFS upload service
	/// </summary>
	internal class FastDFSService : IFastDFSService
	{
		private readonly static ILetterPigeonCage letterPigeon = DependenceInjectService.GetService<ILetterPigeonCage>();
		private readonly IAuthoritionTokenService iAuthoritionTokenService = DependenceInjectService.GetService<IAuthoritionTokenService>();

		/// <summary>
		/// <seealso cref="IFastDFSService.BulkDeliver(ICollection{KeyValuePair{string, Stream}}, Action{FastDfsEndPointOptions})"/>
		/// </summary>
		public DeliverStatusCollection BulkDeliver(ICollection<KeyValuePair<string, Stream>> files, Action<FastDfsEndPointOptions> option = null) =>
			Task.Run(() => letterPigeon.Dispatch(files, InitOptions(option), true)).Result;

		/// <summary>
		/// <seealso cref="IFastDFSService.BulkDeliverAsync(ICollection{KeyValuePair{string, Stream}}, Action{FastDfsEndPointOptions})"/>
		/// </summary>
		public async Task<DeliverStatusCollection> BulkDeliverAsync(ICollection<KeyValuePair<string, Stream>> files, Action<FastDfsEndPointOptions> option = null) =>
			await letterPigeon.Dispatch(files, InitOptions(option), false);

		/// <summary>
		/// <seealso cref="IFastDFSService.Deliver(string, Stream, Action{FastDfsEndPointOptions})"/>
		/// </summary>
		public DeliverStatus Deliver(string name, Stream file, Action<FastDfsEndPointOptions> option = null) =>
			Task.Run(() => letterPigeon.Dispatch(name, file, InitOptions(option), true)).Result;

		/// <summary>
		/// <seealso cref="IFastDFSService.DeliverAsync(string, Stream, Action{FastDfsEndPointOptions})"/>
		/// </summary>
		public async Task<DeliverStatus> DeliverAsync(string name, Stream file, Action<FastDfsEndPointOptions> option = null)  => 
			await letterPigeon.Dispatch(name, file, InitOptions(option), false);

		/// <summary>
		/// <seealso cref="IFastDFSService.GetToken(Action{FastDfsEndPointOptions})"/>
		/// </summary>
		public string GetToken(Action<FastDfsEndPointOptions> option = null) 
		{
			var config = InitOptions(option).AuthoritionComponent ?? throw new ArgumentNullException("No authorized components are used");
			return iAuthoritionTokenService.GetToken(config).Token;
		}

		private FastDfsEndPointOptions InitOptions(Action<FastDfsEndPointOptions> action)
		{
			if (action == null) return DefaultOptionManagement.DefaultOption;

			var option = new FastDfsEndPointOptions
			{
				EndPoint = DefaultOptionManagement.DefaultOption.EndPoint,
				FilePath = DefaultOptionManagement.DefaultOption.FilePath,
				Output = DefaultOptionManagement.DefaultOption.Output,
				Scene = DefaultOptionManagement.DefaultOption.Scene,
				AuthoritionComponent = DefaultOptionManagement.DefaultOption.AuthoritionComponent
			};

			action.Invoke(option);

			return option;
		}
	}

	/// <summary>
	/// Provide basic GoFastDFS upload service
	/// </summary>
	public interface IFastDFSService
	{
		/// <summary>
		/// Upload a single file in synchronously
		/// </summary>
		/// <param name="name">The name of the uploaded file, you need to keep the file after the format suffix name</param>
		/// <param name="file">File stream of uploaded files</param>
		/// <param name="option">Target endpoint setting item, By default and the default frame value is initialized, the default value is used</param>
		/// <returns>Upload receipt of file results</returns>
		DeliverStatus Deliver(string name, Stream file, Action<FastDfsEndPointOptions> option = null);

		/// <summary>
		/// Upload a single file in asynchronously
		/// </summary>
		/// <param name="name">The name of the uploaded file, you need to keep the file after the format suffix name</param>
		/// <param name="file">File stream of uploaded files</param>
		/// <param name="option">Target endpoint setting item, By default and the default frame value is initialized, the default value is used</param>
		/// <returns>Upload receipt of file results</returns>
		Task<DeliverStatus> DeliverAsync(string name, Stream file, Action<FastDfsEndPointOptions> option = null);

		/// <summary>
		/// Upload multiple files in synchronously
		/// </summary>
		/// <param name="files">File name-stream key-value structure set, you can directly use the implementation type of <see cref="IDictionary{String, Stream}"/></param>
		/// <param name="option">Target endpoint setting item, By default and the default frame value is initialized, the default value is used</param>
		/// <returns>Collection of upload file result receipts</returns>
		DeliverStatusCollection BulkDeliver(ICollection<KeyValuePair<string, Stream>> files, Action<FastDfsEndPointOptions> option = null);

		/// <summary>
		/// Upload multiple files in asynchronously
		/// </summary>
		/// <param name="files">File name-stream key-value structure set, you can directly use the implementation type of <see cref="IDictionary{String, Stream}"/></param>
		/// <param name="option">Target endpoint setting item, By default and the default frame value is initialized, the default value is used</param>
		/// <returns>Collection of upload file result receipts</returns>
		Task<DeliverStatusCollection> BulkDeliverAsync(ICollection<KeyValuePair<string, Stream>> files, Action<FastDfsEndPointOptions> option = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="option">Target endpoint setting item, By default and the default frame value is initialized, the default value is used</param>
		/// <returns>a ticket that satisfies the current profile</returns>
		string GetToken(Action<FastDfsEndPointOptions> option = null);
	}
}
