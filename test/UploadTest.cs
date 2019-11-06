using GoFastDFS.Client;
using GoFastDFS.Framework.Test.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFastDFS.Framework.Test
{
	[TestClass]
	public class UploadTest
	{
		private IFastDFSService service = ServiceFactory.GetService();

		[TestMethod]
		//[Timeout(5000)]
		public void SingletonAsynchronous()
		{
			var file = FileSelecter.RandomSingle();
			var result = Task.Run(()=> service.DeliverAsync(file.Key, file.Value)).Result;
			ServiceFactory.WriteText(result, false, true, true);
			Assert.IsTrue(FileSelecter.CheckUpload(result));
		}

		[TestMethod]
		[Timeout(5000)]
		public void SingletonSynchronization()
		{
			var file = FileSelecter.RandomSingle();
			var result = service.Deliver(file.Key, file.Value);
			ServiceFactory.WriteText(result, false, true, true);
			Assert.IsTrue(FileSelecter.CheckUpload(result));
		}

		[TestMethod]
		[Timeout(10000)]
		public void BatchAsynchronous()
		{
			var files = FileSelecter.RandomMultiple();
			Console.WriteLine(files.Count);
			var result = service.BulkDeliverAsync(files).Result;
			ServiceFactory.WriteText(result, false, true);
			Assert.IsTrue(FileSelecter.CheckUpload(result));
		}

		[TestMethod]
		[Timeout(10000)]
		public void BatchSynchronization()
		{
			var files = FileSelecter.RandomMultiple();
			Console.WriteLine(files.Count);
			var result = service.BulkDeliver(files);
			ServiceFactory.WriteText(result, false, true);
			Assert.IsTrue(FileSelecter.CheckUpload(result));
		}

		[TestMethod]
		[Timeout(40000)]
		public void RandomHybrid()
		{
			var infos = FileSelecter.AllFiles;
			var rand = new Random(DateTime.Now.Millisecond);
			var pointer = 0;

			while (pointer < infos.Length)
			{
				bool single = rand.Next(0, 100) < 40;
				bool isAsync = rand.Next(0, 100) < 50;
				if (single)
				{
					var stream = FileSelecter.RandomSingle(infos[pointer++]);
					if (isAsync)
						service.DeliverAsync(stream.Key, stream.Value).ContinueWith(a => ServiceFactory.WriteText(a.Result, isAsync, true));
					else
						ServiceFactory.WriteText(service.Deliver(stream.Key, stream.Value), isAsync, true);
				}
				else
				{

					int num = rand.Next(1, 6);
					List<FileInfo> list = new List<FileInfo>();
					for (int i = 0; i < num && pointer < infos.Length; i++)
					{
						list.Add(infos[pointer]);
						pointer++;
					}
					Dictionary<string, Stream> files = FileSelecter.RandomMultiple(list);
					if (isAsync)
						service.BulkDeliverAsync(files).ContinueWith(a => ServiceFactory.WriteText(a.Result, isAsync));
					else
						ServiceFactory.WriteText(service.BulkDeliver(files), isAsync);
				}
			}
		}
	}
}
