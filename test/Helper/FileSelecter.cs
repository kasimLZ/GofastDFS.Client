using GoFastDFS.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFastDFS.Framework.Test.Helper
{
	public class FileSelecter
	{
		private static FileInfo[] Files = new DirectoryInfo(@"D:\test\images").GetFiles();
		private static Random random = new Random(DateTime.Now.Millisecond);

		public static KeyValuePair<string, Stream> RandomSingle(FileInfo file = null)
		{
			if(file == null)
				file = Files[random.Next(0, Files.Length)];
			return new KeyValuePair<string, Stream>(file.Name, new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		public static Dictionary<string, Stream> RandomMultiple(List<FileInfo> list = null)
		{
			var result = new Dictionary<string, Stream>();
			var max = random.Next(2, Files.Length + 1);
			if (list == null)
				list = Files.ToList();
			else
				max = list.Count;

			for (int i = 0; i < max; i++)
			{
				var file = list[random.Next(0, list.Count)];
				result.Add(file.Name, new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));
				list.Remove(file);
			}
			return result;
		}

		public static FileInfo[] AllFiles => Files;

		public static bool CheckUpload(DeliverStatusCollection collection)
		{
			foreach (var status in collection)
				if (!status.Success)
					return false;
			return true;
		}

		public static bool CheckUpload(DeliverStatus status)
		{
			return status.Success;
		}
	}
}
