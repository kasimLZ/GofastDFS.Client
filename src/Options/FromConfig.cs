using System;

namespace GoFastDFS.Client.Options
{
	/// <summary>
	/// Whether the tag attribute can be read from the configuration file
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	internal class FromConfig : Attribute
	{
		internal FromConfig() { }

		internal FromConfig(string Name) => this.Name = Name;

		/// <summary>
		/// Profile domain name, if no domain name exists
		/// </summary>
		public string Name{ get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public bool Required { get; set; }

		public string FormatHandler { get; set; }
	}
}
