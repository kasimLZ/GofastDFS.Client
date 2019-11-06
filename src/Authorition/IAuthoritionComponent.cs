using GoFastDFS.Client.Authorition;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GoFastDFS.Client
{
	internal interface IAuthoritionComponent
	{
		/// <summary>
		/// The delegate method that intercepts the expiration time from the returned token. If it is empty or the return value is less than or equal to 0, it is not cached.
		/// </summary>
		Func<object, string, int> ExpireTime { get; }

		/// <summary>
		/// Request a token from a remote server
		/// </summary>
		Task<TokenClaim> ApplyToken();

		/// <summary>
		/// <para>Get the feature status of the authentication type</para>
		/// <para>A feature status code is a string consisting of one or a set of parameters and encrypted.</para>
		/// <para>Authentication component with the same signature, will be managed by <see cref="Service.AuthoritionTokenService"/> unified cache</para>
		/// </summary>
		string GetFeatureCode();

		/// <summary>
		/// Add an authorization ticket field in the setup form
		/// </summary>
		void AuthorizeFormSetting(MultipartFormDataContent content, TokenClaim claim);
	}
}
