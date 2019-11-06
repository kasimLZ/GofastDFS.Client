using System;
using System.Collections.Generic;
using System.Text;
#if NETFRAMEWORK
using System.Xml;
#else
using Microsoft.Extensions.Configuration;
#endif
namespace GoFastDFS.Client.Authorition
{
#if NETFRAMEWORK
	internal delegate IAuthoritionComponent CreateComponentHandler(XmlNode Section);
#else
	internal delegate IAuthoritionComponent CreateComponentHandler(IConfigurationSection Section);
#endif


	internal class AuthoritionComponentFactory
	{
		private static readonly Dictionary<AuthoritionType, CreateComponentHandler> CreateFactoryMap =
			new Dictionary<AuthoritionType, CreateComponentHandler> {
				{ AuthoritionType.OIDC, new CreateComponentHandler(OIDCAuthOptionExtension.UseOIDCAuthServer) },
				{ AuthoritionType.Google, new CreateComponentHandler(GoogleAuthOptionExtension.UseGoogleAuthServer) }
			};

#if NETFRAMEWORK
		internal static IAuthoritionComponent CreateAuthoritionComponent(AuthoritionType authorition, XmlNode section) =>
			CreateFactoryMap[authorition].Invoke(section);
#else
		internal static IAuthoritionComponent CreateAuthoritionComponent(AuthoritionType authorition, IConfigurationSection section) =>
			CreateFactoryMap[authorition].Invoke(section);
#endif


	}
}
