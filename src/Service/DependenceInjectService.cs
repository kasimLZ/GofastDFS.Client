using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace GoFastDFS.Client.Service
{
	/// <summary>
	/// A dependency injection service that is limited to use within the current project domain
	/// </summary>
	internal static class DependenceInjectService
	{
		private static IServiceProvider provider { get; }

		static DependenceInjectService()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton<IAuthoritionTokenService, AuthoritionTokenService>()
				.AddSingleton<IHttpClientFactory, HttpClientFactory>()
				.AddSingleton<ILetterPigeonCage, LetterPigeonCage>();
			provider = serviceCollection.BuildServiceProvider();
		}

		/// <summary>
		/// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
		/// </summary>
		/// <typeparam name="T">The type of service object to get.</typeparam>
		/// <param name="InjectMember">
		/// This variable determines whether you want to inject a field with an <see cref="Inject"/> attribute.
		/// <para>Note that the field should be "field" instead of "property" and the field is read-only</para>
		/// </param>
		/// <param name="Recursive">Decide whether to recursively perform field injection</param>
		/// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
		internal static T GetService<T>(bool InjectMember = true, bool Recursive = true) where T : class =>
			InjectMember ? GetService(typeof(T), InjectMember, Recursive) as T : provider.GetService<T>();

		/// <summary>
		///  Get service of type from the <see cref="IServiceProvider"/>.
		/// </summary>
		/// <param name="ServiceType">The type of service object to get.</param>
		/// <param name="InjectMember">
		/// This variable determines whether you want to inject a field with an <see cref="Inject"/> attribute.
		/// <para>Note that the field should be "field" instead of "property" and the field is read-only</para>
		/// </param>
		/// <param name="Recursive">Decide whether to recursively perform field injection</param>
		/// <returns></returns>
		internal static object GetService(Type ServiceType, bool InjectMember = true, bool Recursive = true)
		{
			return InjectMember ? GetServiceWithInjectMember(ServiceType, Recursive) : provider.GetService(ServiceType);
		}

		/// <summary>
		///  Get service and member for service of type from the <see cref="IServiceProvider"/>.
		/// </summary>
		/// <param name="type">The type of service object to get.</param>
		/// <param name="Recursive">Decide whether to recursively perform field injection</param>
		/// <returns></returns>
		private static object GetServiceWithInjectMember(Type type, bool Recursive)
		{
			var service = provider.GetService(type);

			if (service != null)
			{
				foreach (var info in service.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					var inject = info.GetCustomAttribute<Inject>();

					//Fields that have no injection characteristics or are not readonly are not injected
					//If the property contains a get method, you can get the value from the familiarity.
					if (inject == null || !info.IsInitOnly || (inject.NullCheck && info.GetValue(service) != null)) continue;

					info.SetValue(service, Recursive ? GetServiceWithInjectMember(info.FieldType, Recursive) : provider.GetService(info.FieldType));
				}
			}

			return service;
		}
	}

	/// <summary>
	/// <para>Dependency injection attribute</para>
	/// <para>Fields that have this attribute and are read-only can be injected as recursive members, without having to declare them in the constructor</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	internal class Inject : Attribute
	{
		internal Inject() => NullCheck = true;

		internal Inject(bool NullCheck) => this.NullCheck = NullCheck;

		/// <summary>
		/// <para>Decide if to check for null</para>
		/// <para>If you want to check, as long as the injected field is not empty, it will not be injected.</para>
		/// </summary>
		internal bool NullCheck { get; private set; }
	}


}
