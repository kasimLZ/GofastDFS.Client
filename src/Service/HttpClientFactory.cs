using GoFastDFS.Client.Extensions;
using GoFastDFS.Client.Options;
using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace GoFastDFS.Client.Service
{
	/// <summary>
	/// <para>A simple management class that implements HttpClient temporary storage and release</para>
	/// <para>Httpclients on the same interface of the same domain name will be reused. After a certain period of time, 
	/// the object will be automatically released to reduce port and resource usage.</para>
	/// Can refer to <seealso cref="IHttpClientFactory"/>
	/// </summary>
	internal sealed class HttpClientFactory : IHttpClientFactory, IDisposable
	{
		/// <summary>
		/// httpClient storage repository
		/// </summary>
		private readonly MemoryCache HttpClientCollection = new MemoryCache("HttpClientCollection");

		/// <summary>
		/// The timeout period of a single request, the default is 45 seconds. If the request time is too long, the task of the uploading thread will be blocked.
		/// </summary>
		private readonly TimeSpan HttpTimeOut = new TimeSpan(0, 0, 45);

		/// <summary>
		/// <para> httpclient life cycle</para>
		/// <para>Because there is too much system replacement, there will be a 4 minute survival interrupt after release to prevent there from being an outstanding request. </para>
		/// <para>So it will make httpclient survive for 5 minutes, even if httpclient is generated immediately after the release of httpclient, it will not cause a lot of occupied </para>
		/// </summary>
		private readonly TimeSpan HttpClientLifetime = new TimeSpan(0, 5, 0);

		public HttpClientFactory()
		{
			if (DefaultOptionManagement.IsSetDefaultOption)
			{
				//If the default client configuration is set, the resume default link will be initialized.
				HttpClient Default = new HttpClient { Timeout = HttpTimeOut, BaseAddress = HostUri(DefaultOptionManagement.DefaultOption.EndPoint) };
				HttpClientCollection.Set(Default.BaseAddress.AbsoluteUri, Default, new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable });
			}
		}

		/// <summary>
		/// Send an HTTP request as an asynchronous operation.  Can refer to <seealso cref="IHttpClientFactory.SendAsync(Uri, HttpContent, HttpMethod)"/>
		/// </summary>
		/// <param name="url">The full path of the target, including the protocol (http/https) and port, etc.</param>
		/// <param name="content">Request context</param>
		/// <param name="method">Send method (get/post/put/head/delete, etc.)</param>
		/// <returns>Represents a HTTP response message including the status code and data.</returns>
		public async Task<HttpResponseMessage> SendAsync(Uri url, HttpContent content, HttpMethod method = null)
		{
			if (method == null) method = HttpMethod.Post;

			Uri ServerHost = HostUri(url);

			var Client = HttpClientCollection.GetOrCreate(ServerHost.AbsoluteUri, GenerateHttpClient(ServerHost), GenerateCacheItemPolicy());

			return await Client.SendAsync(new HttpRequestMessage { RequestUri = url, Content = content, Method = method });
		}

		/// <summary>
		/// Create a default httpclient delegate method
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private Func<HttpClient> GenerateHttpClient(Uri url) =>
			() =>  new HttpClient { BaseAddress = url, Timeout = HttpTimeOut };


		/// <summary>
		/// Create a cache management policy based on the sliding expiration mechanism
		/// </summary>
		/// <returns></returns>
		private CacheItemPolicy GenerateCacheItemPolicy() =>
			new CacheItemPolicy
			{
				SlidingExpiration = HttpClientLifetime,
				UpdateCallback = arguments => (arguments.UpdatedCacheItem.Value as HttpClient).Dispose()
			};


		/// <summary>
		/// URI formatting method, extract the part of the host protocol + domain name + port in the url
		/// </summary>
		/// <param name="url">url of target</param>
		/// <returns>Host URI of the destination URL</returns>
		private static Uri HostUri(Uri url) => Uri.TryCreate(url, "/", out Uri host) ? host : null;

		public void Dispose()
		{
		 	foreach(var CacheItem in  HttpClientCollection)
				(CacheItem.Value as HttpClient).Dispose();
			HttpClientCollection.Dispose();
		}
	}

	/// <summary>
	/// <para>A simple management class that implements HttpClient temporary storage and release</para>
	/// <para>Httpclients on the same interface of the same domain name will be reused. After a certain period of time, 
	/// the object will be automatically released to reduce port and resource usage.</para>
	/// </summary>
	internal interface IHttpClientFactory : IDisposable
	{
		/// <summary>
		/// Send an HTTP request as an asynchronous operation.
		/// </summary>
		/// <param name="url">The full path of the target, including the protocol (http/https) and port, etc.</param>
		/// <param name="content">Request context</param>
		/// <param name="method">Send method (get/post/put/head/delete, etc.)</param>
		/// <returns>Represents a HTTP response message including the status code and data.</returns>
		Task<HttpResponseMessage> SendAsync(Uri url, HttpContent content, HttpMethod method = null);
	}
}
