using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GoFastDFS.Client.Authorition.Customize
{
	internal class CustomizeAuthComponent : AuthoritionComponentBase, IAuthoritionComponent
	{

		public Func<object, string, int> ExpireTime => throw new NotImplementedException();

		public void AuthorizeFormSetting(MultipartFormDataContent content, TokenClaim claim)
		{
			throw new NotImplementedException();
		}

		public string GetFeatureCode()
		{
			throw new NotImplementedException();
		}

		public async Task<TokenClaim> ApplyToken()
		{
			throw new NotImplementedException();
		}
	}
}
