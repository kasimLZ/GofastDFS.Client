using System;
using System.Runtime.Caching;

namespace GoFastDFS.Client.Extensions
{
	internal static class MemoryCacheExtensions
	{
		internal static T Get<T>(this MemoryCache cache, string key, string regionName = null) where T : class => cache.Get(key, regionName) as T;


		internal static T GetOrCreate<T>(this MemoryCache cache, string key, Func<T> Creater, DateTimeOffset Expire, string regionName = null) where T : class =>
			cache.GetOrCreate(key, Creater, new CacheItemPolicy { AbsoluteExpiration = Expire }, regionName);

		internal static T GetOrCreate<T>(this MemoryCache cache, string key, Func<T> Creater, TimeSpan Expire, string regionName = null) where T : class =>
			cache.GetOrCreate(key, Creater, new CacheItemPolicy { SlidingExpiration = Expire }, regionName);


		internal static T GetOrCreate<T>(this MemoryCache cache, string key, Func<T> Creater, CacheItemPolicy policy, string regionName = null) where T : class
		{
			T item = cache.Get<T>(key, regionName);
			if (item == null)
			{
				lock (cache)
				{
					item = cache.Get<T>(key, regionName);
					if (item == null)
					{
						item = Creater.Invoke();
						cache.Set(key, item, policy, regionName);
					}
				}
			}
			return item;
		}
	}
}
