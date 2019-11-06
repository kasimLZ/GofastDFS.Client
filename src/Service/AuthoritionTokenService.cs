using GoFastDFS.Client.Authorition;
using GoFastDFS.Client.Extensions;
using GoFastDFS.Client.Options;
using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;


namespace GoFastDFS.Client.Service
{
	/// <summary>
	/// Upload ticket management service
	/// </summary>
	internal sealed class AuthoritionTokenService : IAuthoritionTokenService, IDisposable
	{
		/// <summary>
		/// Default wait time for read-write locks
		/// </summary>
		private const int READ_WAIT_LIMIT = 10000;

		private readonly string CacheName;
		private readonly MemoryCache TokenCache;

		public AuthoritionTokenService()
		{
			CacheName = Guid.NewGuid().ToString();
			TokenCache = new MemoryCache(CacheName);
		}

		/// <summary>
		/// <see cref="IAuthoritionTokenService.GetToken(IAuthoritionComponent)"/>
		/// </summary>
		public TokenClaim GetToken(IAuthoritionComponent component)
		{
			var feature = component.GetFeatureCode();

			//Try to remove the read-write lock from cache
			var locker = TokenCache.GetOrCreate(feature + "_locker", () => new ReaderWriterLockSlim(), null);

			TokenClaim Claim = null;

			do
			{
				Claim = locker.RetryReadWithLocker(
						 () => TokenCache.Get<TokenClaim>(feature), READ_WAIT_LIMIT, DefaultOptionManagement.MaximumNumberOfRetries);

				//Return directly if the ticket exists
				if (Claim != null && Claim.Expires.HasValue) return Claim;

			} while (!locker.TryEnterWriteLock(10));

			//Read and write cannot acquire the state normally in the case of asynchronous, 
			//so the process of acquiring the token is executed in a synchronous blocking manner.
			Claim = Task.Run(() => GetClaimTryToCache(TokenCache, feature, component)).Result;

			locker.ExitWriteLock();

			return Claim;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Cache"></param>
		/// <param name="feature"></param>
		/// <param name="component"></param>
		/// <returns></returns>
		private static async Task<TokenClaim> GetClaimTryToCache(MemoryCache Cache, string feature, IAuthoritionComponent component)
		{
			CacheItemPolicy policy = null;

			var Claim = await component.ApplyToken();

			//If the new token expiration time has expired, then the current token won't be cached.
			if (Claim.Expires.HasValue)
			{
				if (Claim.Expires.Value <= DateTime.Now)
					Claim.Expires = null;
				else
					policy = new CacheItemPolicy { AbsoluteExpiration = Claim.Expires.Value };
			}

			Cache.Set(feature, Claim, policy);

			return Claim;
		}

		public void Dispose() => TokenCache.Dispose();
		
	}

	/// <summary>
	/// Upload ticket management interface
	/// </summary>
	internal interface IAuthoritionTokenService
	{
		/// <summary>
		/// Try to get the token from the cache, or generate from the component if there is no cache
		/// </summary>
		TokenClaim GetToken(IAuthoritionComponent component);
	}
}
