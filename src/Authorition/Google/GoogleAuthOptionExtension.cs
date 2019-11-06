using GoFastDFS.Client.Authorition.Google;
using System;
#if NETFRAMEWORK
using System.Xml;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace GoFastDFS.Client
{
	public static class GoogleAuthOptionExtension
	{
		public static IGoogleAuthBuilder UseGoogleAuthServer(this FastDfsEndPointOptions option) => new GoogleAuthBuilder(option);


#if NETFRAMEWORK
		internal static IAuthoritionComponent UseGoogleAuthServer(XmlNode section)
		{
			var Component = new GoogleAuthComponent();
			FastDFSConfigurationSection.FormatFromConfig(Component, section);
			return Component;
		}
#else
		internal static IAuthoritionComponent UseGoogleAuthServer(IConfigurationSection section)
		{
			var Component = new GoogleAuthComponent();
			FastDFSConfigurationSection.FormatFromConfig(Component, section);
			return Component;
		}
#endif
	}
}
