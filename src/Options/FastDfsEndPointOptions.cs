using GoFastDFS.Client.Options;
using System;

namespace GoFastDFS.Client
{
	/// <summary>
	/// Client endpoint settings
	/// </summary>
	public class FastDfsEndPointOptions
	{

		/// <summary>
		/// Server address, format reference <code>http://127.0.0.1:8001/upload</code>
		/// </summary>
		[FromConfig]
		public Uri EndPoint { get; set; }

		/// <summary>
		/// <para>File custom path</para>
		/// 
		/// </summary>
		[FromConfig]
		public string FilePath { get; set; }

		/// <summary>
		/// The data type returned by the GoFastDFS server when the upload was successful
		/// </summary>
		[FromConfig]
		public FastDfsOutput Output { get; set; } = FastDfsOutput.Json;

		/// <summary>
		/// <para>The scene of the currently uploaded file.</para>
		/// <para>For details on how to use the scene pool, please refer to the official documentation of GoFastDFS.</para>
		/// <para>https://sjqzhang.gitee.io/go-fastdfs</para>
		/// </summary>
		[FromConfig]
		public string Scene { get; set; }

		/// <summary>
		/// Authentication and authorization server address<para></para>
		/// only when the field is not empty, apply for authorization before uploading data
		/// </summary>
		internal IAuthoritionComponent AuthoritionComponent { get; set; }

	}

	/// <summary>
	/// GoFastDFS response data structure
	/// </summary>
	public enum FastDfsOutput
	{
		[StringEnumValue("json")]
		Json,
		[StringEnumValue("text")]
		Text
	}
}
