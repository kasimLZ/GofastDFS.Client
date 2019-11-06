using System;
using Newtonsoft.Json.Linq;

namespace GoFastDFS.Client.Authorition.Google
{
	internal class GoogleAuthBuilder : IGoogleAuthBuilder
	{
		private const string SET_AUTHORIZED_SERVER = "Authorized server address has been set";
		private const string SET_AUTHORIZED_KEY = "Authorization key has been set";

		private readonly GoogleAuthComponent Component;

		internal GoogleAuthBuilder(FastDfsEndPointOptions options) => options.AuthoritionComponent = Component = new GoogleAuthComponent();

		public IGoogleAuthBuilder SetAuthorityServer(Uri url)
		{
			if (Component.Mode == GoogleAuthComponent.GAMode.None)
				Component.AuthorityServer = url;
			else if (Component.Mode == GoogleAuthComponent.GAMode.RemoteAccess)
				throw new ApplicationException(SET_AUTHORIZED_SERVER);
			else
				throw new ApplicationException(SET_AUTHORIZED_KEY);
			return this;
		}

		public IGoogleAuthBuilder SetCodeLeftTime(int second)
		{
			Component.ExpireSecond = second;
			return this;
		}

		public IGoogleAuthBuilder SetSecretKey(string secretKey)
		{
			if (Component.Mode == GoogleAuthComponent.GAMode.None)
				Component.SecretKey = secretKey;
			else if (Component.Mode == GoogleAuthComponent.GAMode.Generation)
				throw new ApplicationException(SET_AUTHORIZED_KEY);
			else
				throw new ApplicationException(SET_AUTHORIZED_SERVER);
			return this;
		}
	}

	public interface IGoogleAuthBuilder
	{
		IGoogleAuthBuilder SetAuthorityServer(Uri url);

		IGoogleAuthBuilder SetSecretKey(string secretKey);

		IGoogleAuthBuilder SetCodeLeftTime(int second);
	}
}
