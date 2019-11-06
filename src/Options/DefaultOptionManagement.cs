using System;
using System.Collections.Generic;
using System.Text;

namespace GoFastDFS.Client.Options
{
	internal class DefaultOptionManagement
	{
		private static FastDfsSChemeOptions _DefaultOption_ = null;

		internal static bool IsSetDefaultOption = _DefaultOption_ != null;

		internal static FastDfsSChemeOptions DefaultOption
		{
			get => GetHandler();
			set => SetHandler(value);
		}

		private delegate void SetDefaultHandler(FastDfsSChemeOptions options);

		private static SetDefaultHandler SetHandler = (options) => {
			_DefaultOption_ = options;
			IsSetDefaultOption = true;
			SetHandler = (opt) => throw new InvalidOperationException("Cannot initialize initialization operation parameters");
			GetHandler = () => _DefaultOption_;
		};

		private delegate FastDfsSChemeOptions GetDefaultHandler();

		private static GetDefaultHandler GetHandler = () =>
		{
			throw new InvalidOperationException("Uninitialized processing operation parameters");
		};

		internal static int MaxConnetPool => IsSetDefaultOption ? DefaultOption.MaxConnetPool : FastDfsSChemeOptions._MaxConnetPool_;

		internal static int MaximumNumberOfRetries => IsSetDefaultOption ? DefaultOption.MaximumNumberOfRetries : FastDfsSChemeOptions._MaximumNumberOfRetries_;

		internal static int RetryMillisecond => IsSetDefaultOption ? DefaultOption.RetryMillisecond : FastDfsSChemeOptions._RetryMillisecond_;
	}
}
