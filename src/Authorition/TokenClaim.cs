using System;

namespace GoFastDFS.Client.Authorition
{
	internal class TokenClaim
	{

		internal string Raw { get; set; }

		internal string Token { get; set; }

		internal DateTime? Expires { get; set; }
	}
}
