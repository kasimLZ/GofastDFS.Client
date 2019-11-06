
using GoFastDFS.Client.Options;

namespace GoFastDFS.Client
{
	/// <summary>
	/// FastDfs client global configuration options
	/// </summary>
	public class FastDfsSChemeOptions : FastDfsEndPointOptions
	{
		/// <summary>
		/// The maximum number of links, the total number of processes that can be uploaded simultaneously.
		/// </summary>
		[FromConfig]
		public int MaxConnetPool { get; set; } = _MaxConnetPool_;
		internal const int _MaxConnetPool_ = 10;

		/// <summary>
		/// If an exception is thrown when a lock or other problem occurs, the number of retries
		/// </summary>
		[FromConfig("RetryNumber")]
		public int MaximumNumberOfRetries { get; set; } = _MaximumNumberOfRetries_;
		internal const int _MaximumNumberOfRetries_ = 5;

		/// <summary>
		/// The number of milliseconds to wait between each retries when a timeout is thrown when a lock or other problem is encountered.
		/// </summary>
		[FromConfig]
		public int RetryMillisecond { get; set; } = _RetryMillisecond_;
		internal const int _RetryMillisecond_ = 1000;
	}
}
