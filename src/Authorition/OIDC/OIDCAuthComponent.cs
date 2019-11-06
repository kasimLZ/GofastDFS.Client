using GoFastDFS.Client.Options;
using GoFastDFS.Client.Service;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GoFastDFS.Client.Authorition.OIDC
{
	internal class OIDCAuthComponent : AuthoritionComponentBase, IAuthoritionComponent
	{
		internal Uri AuthorityServer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, string> AuthoritionTicket { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// 
		/// </summary>
		public string CredentialKey { get; set; } = "access_token";

		/// <summary>
		/// <para>Method for obtaining a ticket from a token obtained from an authentication server</para>
		/// </summary>
		public Func<JObject, string, string> Credential { get; set; } = (TokenObject, CredentialKey) => TokenObject[CredentialKey].ToString();

		/// <summary>
		/// 
		/// </summary>
		public string ExpireTimeKey { get; set; } = "expires_in";

		/// <summary>
		/// <para>Method of obtaining a validity period（second） from a token obtained by an authentication server.</para>
		/// <para>When the expiration time is less than or equal to zero, the ticket will not be cached.</para>
		/// </summary>
		public Func<object, string, int> ExpireTime { get; set; } =
			(TokenObject, ExpireTimeKey) =>
			{
				var expireTime = (TokenObject as JObject)[ExpireTimeKey].ToString();
				return string.IsNullOrWhiteSpace(expireTime) || !int.TryParse(expireTime, out int time) ? -1 : time;
			};


		public async Task<TokenClaim> ApplyToken()
		{
			var content = new MultipartFormDataContent();

			foreach (var item in AuthoritionTicket)
				content.Add(new StringContent(item.Value), item.Key);

			var response = await DependenceInjectService.GetService<IHttpClientFactory>().SendAsync(AuthorityServer, content);

			var Claim = new TokenClaim { Raw = await response.Content.ReadAsStringAsync() };

			var json = JObject.Parse(Claim.Raw);

			Claim.Token = Credential.Invoke(json, CredentialKey);

			var ExpireSecond = ExpireTime.Invoke(json, ExpireTimeKey);

			if (ExpireSecond > 0)
				Claim.Expires = DateTime.Now.AddSeconds(ExpireSecond);

			return Claim;
		}

		private string FeatureCode = null;

		public string GetFeatureCode()
		{
			if (FeatureCode != null) return FeatureCode;

			StringBuilder feature = new StringBuilder(AuthorityServer.AbsoluteUri);

			foreach (var key in AuthoritionTicket.Keys.OrderBy(a => a))
				feature.Append($"{key}{AuthoritionTicket[key]}");

			FeatureCode = MD5Encrypt(feature.ToString());

			return FeatureCode;
		}

		public void AuthorizeFormSetting(MultipartFormDataContent Content, TokenClaim Claim)
		{
			Content.Add(new StringContent(Claim.Token), "auth_token");
		}
	}
}
