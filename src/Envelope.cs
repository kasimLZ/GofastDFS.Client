using System;
using System.IO;
using System.Threading;

namespace GoFastDFS.Client
{
	/// <summary>
	/// Send a single task instance of a file
	/// </summary>
	internal sealed class Envelope : IDisposable
	{
		/// <summary>
		/// Upload configuration used by the task
		/// </summary>
		internal FastDfsEndPointOptions Options { get; set; }

		/// <summary>
		/// Upload file name
		/// </summary>
		internal string FileName { get; set; }

		/// <summary>
		/// File stream of uploaded files
		/// </summary>
		internal Stream Cargo { get; set; }

		/// <summary>
		/// Spin lock for value, when the receipt does not complete the blocking value thread
		/// </summary>
		private AutoResetEvent keeper = new AutoResetEvent(false);

		/// <summary>
		/// Upload receipt, if it is empty, it means the task has not been completed or has not started yet
		/// </summary>
		private DeliverStatus? status = null;

		/// <summary>
		/// Get or set the upload receipt, if the assignment will automatically release the spin lock
		/// </summary>
		internal DeliverStatus? Status
		{
			get 
			{
				if (!status.HasValue)
				{
					if (keeper == null) 
						keeper = new AutoResetEvent(false);
					keeper.WaitOne();
				}
				return status.Value;
			}
			set
			{
				if (!value.HasValue && keeper == null)
					keeper = new AutoResetEvent(false);
				status = value;
				keeper.Set();
			}
		}

		public void Dispose() => keeper.Dispose();
	}
}
