using System;

namespace GoFastDFS.Client
{
	/// <summary>
	/// Upload status receipt for a single file
	/// </summary>
	public struct DeliverStatus
	{
		/// <summary>
		/// Whether the transmission is successful depends on the response result of the current FastDFS server
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// File Name
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// If the transmission fails, you can see all the error messages intercepted by the system here.
		/// </summary>
		public Exception ErrorMessage { get; set; }

		/// <summary>
		/// Return the payload of the message
		/// </summary>
		public PayloadInfo PayLoad { get; set; }

		/// <summary>
		/// Original response information
		/// </summary>
		public string RawResponse { get; set; }
	}
}
