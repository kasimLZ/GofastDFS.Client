using GoFastDFS.Client.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoFastDFS.Client
{
	public static class FastDfsServiceFactory
	{
		public static IFastDFSService GetService() => DependenceInjectService.GetService<IFastDFSService>();
	}
}
