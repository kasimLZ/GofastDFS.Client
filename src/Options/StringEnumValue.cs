using System;

namespace GoFastDFS.Client
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	internal class StringEnumValue : Attribute
	{
		internal StringEnumValue(string Value) => this.Value = Value;


		internal string Value { get; set; }
	}

	internal static class StringEnumValueExtensions
	{
		internal static string StringValue(this Enum e)
		{
			var values = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(StringEnumValue), false);
			if (values.Length == 0) return string.Empty;
			return !(values[0] is StringEnumValue value) ? string.Empty : value.Value;
		}
	}
}
