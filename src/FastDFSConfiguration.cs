using GoFastDFS.Client.Options;
using System;
#if NETFRAMEWORK
using System.Configuration;
#else
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace GoFastDFS.Client
{
	/// <summary>
	/// Default configuration initialization configuration class
	/// </summary>
	public static class FastDFSConfiguration
	{
		private const string DefaultFastDFSConfigurationName = "FastDFSConfiguration";

#if NETFRAMEWORK
		/// <summary>
		/// Register default configuration parameters
		/// </summary>
		/// <param name="action">Default parameter configuration method</param>
		public static void AddFastDFSClientDefaultOption(Action<FastDfsSChemeOptions> action)
		{
			var options =  new FastDfsSChemeOptions();
			action.Invoke(options);
			DefaultOptionManagement.DefaultOption = options;
		}

		/// <summary>
		/// Read the default configuration from the project configuration file
		/// </summary>
		public static void AddFastDFSClientDefaultOptionFromConfig()
		{
			DefaultOptionManagement.DefaultOption = ConfigurationManager.GetSection(DefaultFastDFSConfigurationName) as FastDfsSChemeOptions;
		}
#else
		/// <summary>
		/// Register default configuration parameters
		/// </summary>
		/// <param name="services"></param>
		/// <param name="action"></param>
		public static IServiceCollection AddFastDFSClientDefaultOption(this IServiceCollection services, Action<FastDfsSChemeOptions> action)
		{
			var option = new FastDfsSChemeOptions();
			action.Invoke(option);
			return AddFastDFSClientDefaultOption(services, option);
		}

		/// <summary>
		/// Read the default configuration from the project configuration file
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		public static IServiceCollection AddFastDFSClientDefaultOption(this IServiceCollection services, IConfiguration configuration)
		{
			return AddFastDFSClientDefaultOption(services, FastDFSConfigurationSection.Create(configuration.GetSection(DefaultFastDFSConfigurationName)));
		}

		private static IServiceCollection AddFastDFSClientDefaultOption(IServiceCollection services, FastDfsSChemeOptions option)
		{
			services.AddScoped<IFastDFSService, FastDFSService>();
			DefaultOptionManagement.DefaultOption = option;
			return services;
		}
#endif
	}
}
