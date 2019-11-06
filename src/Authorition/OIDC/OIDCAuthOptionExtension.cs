using GoFastDFS.Client.Authorition.OIDC;
using System;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Xml;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace GoFastDFS.Client
{
	public static class OIDCAuthOptionExtension
	{
		public static IOIDCAuthBuilder UseOIDCAuthServer(this FastDfsEndPointOptions option, Uri uri) => new OIDCAuthBuilder(option).SetUrl(uri);


#if NETFRAMEWORK
		internal static IAuthoritionComponent UseOIDCAuthServer(XmlNode section) 
		{
			var Component = new OIDCAuthComponent();
			FastDFSConfigurationSection.FormatFromConfig(Component, section);
			return Component;
		}
#else
		internal static IAuthoritionComponent UseOIDCAuthServer(IConfigurationSection section)
		{
			var Component = new OIDCAuthComponent();
			FastDFSConfigurationSection.FormatFromConfig(Component, section);
			return Component;
		}
#endif
	}
}
