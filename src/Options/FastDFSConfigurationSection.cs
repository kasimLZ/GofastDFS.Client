using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GoFastDFS.Client.Options;
using GoFastDFS.Client.Authorition;
#if NETFRAMEWORK

using System.Configuration;
using System.Xml;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace GoFastDFS.Client
{
	/// <summary>
	/// 
	/// </summary>
	public class FastDFSConfigurationSection
#if NETFRAMEWORK
		: IConfigurationSectionHandler
#endif
	{
		private const string DefaultAuthoriteNode = "Authorition";

		private const string UnKnownAuthoriteType = "Find no authorization type";

#if NETFRAMEWORK
		/// <summary>
		/// Create a configuration object from a configuration file
		/// </summary>
		/// <param name="parent">Parent object</param>
		/// <param name="configContext">Profile context</param>
		/// <param name="section">Configuration node</param>
		/// <returns>configuration object</returns>
		public object Create(object parent, object configContext, XmlNode section)
		{
			var options = new FastDfsSChemeOptions();

			FormatFromConfig(options, section);

			var AuthSection = section.SelectSingleNode(DefaultAuthoriteNode);

			if (AuthSection != null)
			{
				if (!Enum.TryParse(AuthSection.Attributes["Type"]?.Value, out AuthoritionType type))
					throw new Exception(UnKnownAuthoriteType);
				options.AuthoritionComponent = AuthoritionComponentFactory.CreateAuthoritionComponent(type, AuthSection);
			}

			return options;
		}

		/// <summary>
		/// Serialize configuration objects according to <see cref="XmlNode"/> and <see cref="FromConfig"/>
		/// </summary>
		/// <typeparam name="T">Target type</typeparam>
		/// <param name="Target">Serialized assignment target</param>
		/// <param name="section">Configuration node</param>
		internal static void FormatFromConfig<T>(T Target, XmlNode section)
		{
			var props = GetConfigProperties<T>();

			foreach (var info in props)
			{
				FromConfig config = info.GetCustomAttribute<FromConfig>();

				if(!string.IsNullOrEmpty( config.FormatHandler))
				{
					var method = typeof(T).GetMethod(config.FormatHandler, BindingFlags.Public | BindingFlags.Instance);
					if(method != null)
					{
						method.Invoke(Target, new object[] { section });
						continue;
					}
				}

				string name = config.Name ?? info.Name;
				var attribute = section.Attributes[name];
				if (attribute != null)
				{
					if (info.PropertyType.BaseType == typeof(Enum))
						info.SetValue(Target, Enum.Parse(info.PropertyType, attribute.Value));
					if (info.PropertyType == typeof(Uri))
						info.SetValue(Target, new Uri(attribute.Value));
					else
						info.SetValue(Target, Convert.ChangeType(attribute.Value, info.PropertyType));
				}

				else if (config.Required)
				{
					throw new ArgumentNullException($"Missing required attributes: {name}");
				}
			}
		}

#else
		/// <summary>
		/// Create a configuration object from a configuration file
		/// </summary>
		/// <param name="section">Configuration node</param>
		/// <returns>configuration object</returns>
		internal static FastDfsSChemeOptions Create(IConfigurationSection section)
		{
			var options = new FastDfsSChemeOptions();

			FormatFromConfig(options, section);

			var AuthSection = section.GetSection(DefaultAuthoriteNode);

			if (AuthSection != null)
			{
				if (!Enum.TryParse(AuthSection.GetSection("Type")?.Value, out AuthoritionType type))
					throw new Exception(UnKnownAuthoriteType);
				options.AuthoritionComponent = AuthoritionComponentFactory.CreateAuthoritionComponent(type, AuthSection);
			}

			return options;
		}

		/// <summary>
		/// Serialize configuration objects according to <see cref="IConfigurationSection"/> and <see cref="FromConfig"/>
		/// </summary>
		/// <typeparam name="T">Target type</typeparam>
		/// <param name="Target">Serialized assignment target</param>
		/// <param name="section">Configuration node</param>
		internal static void FormatFromConfig<T>(T Target, IConfigurationSection section)
		{
			var props = GetConfigProperties<T>();

			foreach (var info in props)
			{
				var config = info.GetCustomAttribute<FromConfig>();
				if (!string.IsNullOrEmpty(config.FormatHandler))
				{
					var method = typeof(T).GetMethod(config.FormatHandler, BindingFlags.Public | BindingFlags.Instance);
					if (method != null)
					{
						method.Invoke(Target, new object[] { section });
						continue;
					}
				}

				string name = info.Name ?? info.Name;
				var attribute = section.GetSection(name);
				if (attribute != null)
				{
					if (info.PropertyType.BaseType == typeof(Enum))
						info.SetValue(Target, Enum.Parse(info.PropertyType, attribute.Value));
					else
						info.SetValue(Target, Convert.ChangeType(attribute.Value, info.PropertyType));
				}
				else if (config.Required)
				{
					throw new ArgumentNullException($"Missing required attributes: {name}");
				}
			}
		}
#endif

		/// <summary>
		/// Filter all property with <see cref="FromConfig"/> attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private static IEnumerable<PropertyInfo> GetConfigProperties<T>() =>
				typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
					.Where(a => a.GetCustomAttribute<FromConfig>() != null);



	}
}
