using System;
using Newtonsoft.Json.Linq;

namespace GoFastDFS.Client.Authorition.OIDC
{
	internal class OIDCAuthBuilder : IOIDCAuthBuilder
	{
		private readonly OIDCAuthComponent Component;

		internal OIDCAuthBuilder(FastDfsEndPointOptions sptions) =>
			sptions.AuthoritionComponent = Component = new OIDCAuthComponent();
		

		internal IOIDCAuthBuilder SetUrl (Uri uri)
		{
			Component.AuthorityServer = uri;
			return this;
		}

		public IOIDCAuthBuilder AddTicketItem(string key, string value)
		{
			Component.AuthoritionTicket.Add(key, value);
			return this;
		}

		public IOIDCAuthBuilder CredentialSelecter(string CredentialKey)
		{
			Component.CredentialKey = CredentialKey;
			return this;
		}

		public IOIDCAuthBuilder CredentialSelecter(Func<JObject, string, string> Credential)
		{
			Component.Credential = Credential;
			return this;
		}

		public IOIDCAuthBuilder ExpireTimeSelecter(string ExpireTimeKey)
		{
			Component.ExpireTimeKey = ExpireTimeKey;
			return this;
		}

		public IOIDCAuthBuilder ExpireTimeSelecter(Func<object, string, int> ExpireTime)
		{
			Component.ExpireTime = ExpireTime;
			return this;
		}

		public IOIDCAuthBuilder SetTicket(object ticket)
		{
			var prop = ticket.GetType().GetProperties();
			foreach(var info in prop)
				AddTicketItem(info.Name, info.GetValue(ticket).ToString());
			return this;
		}
	}

	public interface IOIDCAuthBuilder
	{
		IOIDCAuthBuilder SetTicket(object ticket);

		IOIDCAuthBuilder AddTicketItem(string key, string value);

		IOIDCAuthBuilder CredentialSelecter(string CredentialKey);

		IOIDCAuthBuilder CredentialSelecter(Func<JObject, string, string> Credential);

		IOIDCAuthBuilder ExpireTimeSelecter(string ExpireTimeKey);

		IOIDCAuthBuilder ExpireTimeSelecter(Func<object, string, int> ExpireTime);
	}
}
