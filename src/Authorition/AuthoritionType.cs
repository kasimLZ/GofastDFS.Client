namespace GoFastDFS.Client
{
	/// <summary>
	/// The base available type of the authentication component
	/// </summary>
	internal enum AuthoritionType
	{
		/// <summary>
		/// Custom authentication mode
		/// </summary>
		Customize,

		/// <summary>
		/// Google Dynamic Code Dynamic Authorization Mode
		/// </summary>
		Google,

		/// <summary>
		/// OIDC authorized service mode
		/// </summary>
		OIDC
	}
}
