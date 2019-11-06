using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoFastDFS.Client.Extensions
{
	internal static class ReaderWriterLockSlimExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="locker"></param>
		/// <param name="Reader"></param>
		/// <param name="WaitMilliSeconds"></param>
		/// <param name="RetryTime"></param>
		/// <param name="RetryMilliSeconds"></param>
		/// <exception cref="Exception">
		/// 
		/// </exception>
		/// <returns></returns>
		internal static T RetryReadWithLocker<T>(
			this ReaderWriterLockSlim locker,
			Func<T> Reader,
			int WaitMilliSeconds,
			int RetryTime,
			int RetryMilliSeconds = 1000)
		{
			int counter = 0;
			bool status;

			RETRY_LOCK_LOADER:
			status = locker.TryEnterReadLock(WaitMilliSeconds);
			if (!status) Thread.Sleep(RetryMilliSeconds);

			if (!status)
			{
				if (counter <= RetryTime)
				{
					counter++;
					goto RETRY_LOCK_LOADER;
				}

				throw new ApplicationException("");
			}

			T result = Reader.Invoke();
			locker.ExitReadLock();

			return result;
		}
	}
}
