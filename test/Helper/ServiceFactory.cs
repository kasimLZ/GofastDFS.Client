using GoFastDFS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFastDFS.Framework.Test.Helper
{
	public class ServiceFactory
	{
		public static int Counter = 0;

		static ServiceFactory()
		{
			FastDFSConfiguration.AddFastDFSClientDefaultOption(a =>
			{
				a.EndPoint = new Uri("http://127.0.0.1:8001/upload");
				a.UseOIDCAuthServer(new Uri("http://localhost:8100/connect/token"))
					.SetTicket(new { client_id = "GoFastDFS", client_secret = "Bdyh.FastDFS.Secret", grant_type = "client_credentials" });
			});

			
		}

		public static IFastDFSService GetService()
		{
			return new FastDFSService();
		}

		public static void WriteText(DeliverStatusCollection collection, bool isAsync, bool isError = false)
		{
			for (var i = 0; i < collection.Count; i++)
				WriteText(collection[i], isAsync, false, isError);
		}

		public static void WriteText(DeliverStatus status, bool isAsync, bool isSingle, bool isError = false)
		{
			if (isError && status.Success) return;

			StringBuilder sb = new StringBuilder();
			sb.Append($"FileName: {status.FileName} \r\n");
			sb.Append($"isAsync: {isAsync} \r\n");
			sb.Append($"isSingle: {isSingle} \r\n");
			sb.Append($"Success: {status.Success} \r\n");
			if (status.Success)
			{
				sb.Append($"Url: {status.PayLoad.Url} \r\n");
			}
			else
			{
				var e = status.ErrorMessage;
				sb.Append($"ErrorMessage: \r\n");
				while (e != null)
				{
					sb.Append($"\t {e.Message}");
					e = e.InnerException;
				}
			}
			sb.Append("\r\n");
			Console.WriteLine(sb.ToString());
		}
	}
}
