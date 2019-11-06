using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GoFastDFS.Client.Service;
using Newtonsoft.Json.Linq;

namespace GoFastDFS.Client.Authorition.Google
{
	internal class GoogleAuthComponent : AuthoritionComponentBase, IAuthoritionComponent
	{
		internal GAMode Mode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		internal string SecretKey { get; set; }

		/// <summary>
		/// 
		/// </summary>
		internal Uri AuthorityServer { get; set; }

		/// <summary>
		/// 
		/// </summary>
		internal int ExpireSecond { get; set; } = 30;

		public Func<object, string, int> ExpireTime => (a, b) => ExpireSecond;

		public async Task<TokenClaim> ApplyToken()
		{
			TimeSpan spend = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			if (Mode == GAMode.RemoteAccess)
			{
				var response = await DependenceInjectService.GetService<IHttpClientFactory>().SendAsync(AuthorityServer, null, HttpMethod.Get);
				var Claim = new TokenClaim();
				Claim.Raw = Claim.Token = await response.Content.ReadAsStringAsync();
				Claim.Expires = DateTime.Now.AddSeconds(spend.TotalSeconds % ExpireSecond);
				return Claim;
			}
			else if (Mode == GAMode.Generation)
				return GetCodeInternal(SecretKey, spend);
			else
				throw new NotSupportedException("The authentication component has not been initialized yet. Please set the server address or Secretkey.");
		}

		public void AuthorizeFormSetting(MultipartFormDataContent content, TokenClaim Token)
		{
			content.Add(new StringContent(Token.Token), "auth_token");
		}

		private string FeatureCode = null;

		public string GetFeatureCode()
		{
			if (string.IsNullOrWhiteSpace(FeatureCode))
			{
				var builder = new StringBuilder();
				builder.Append(Mode.ToString());
				builder.Append("_");
				switch (Mode)
				{
					case GAMode.Generation: builder.Append(SecretKey); break;
					case GAMode.RemoteAccess: builder.Append(AuthorityServer.AbsoluteUri); break;
					default: throw new Exception("");
				}
				FeatureCode = MD5Encrypt(builder.ToString());
			}
			return FeatureCode;
		}

		internal enum GAMode
		{
			None,
			RemoteAccess,
			Generation
		}

		private TokenClaim GetCodeInternal(string secret, TimeSpan spend)
		{
			var Claim = new TokenClaim();
			var Second = (ulong)ExpireSecond;

			ulong chlg = (ulong)(spend.TotalSeconds / Second);
			Claim.Expires = DateTime.Now.AddSeconds(spend.TotalSeconds % Second);


			byte[] challenge = new byte[8];
			for (int j = 7; j >= 0; j--)
			{
				challenge[j] = (byte)((int)chlg & 0xff);
				chlg >>= 8;
			}

			var key = Base32ToBytes(secret);
			for (int i = secret.Length; i < key.Length; i++)
			{
				key[i] = 0;
			}

			HMACSHA1 mac = new HMACSHA1(key);
			var hash = mac.ComputeHash(challenge);

			int offset = hash[hash.Length - 1] & 0xf;

			int truncatedHash = 0;
			for (int j = 0; j < 4; j++)
			{
				truncatedHash <<= 8;
				truncatedHash |= hash[offset + j];
			}

			truncatedHash &= 0x7FFFFFFF;
			truncatedHash %= 1000000;

			string code = truncatedHash.ToString();
			Claim.Raw = Claim.Token = code.PadLeft(6, '0');

			return Claim;
		}
	}
}
