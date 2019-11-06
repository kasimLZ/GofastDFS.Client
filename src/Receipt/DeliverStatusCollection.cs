using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GoFastDFS.Client
{
	/// <summary>
	/// Receipt collection for bulk upload files
	/// </summary>
	public class DeliverStatusCollection : ReadOnlyCollection<DeliverStatus>, IReadOnlyCollection<DeliverStatus>
	{
		internal DeliverStatusCollection(IList<DeliverStatus> statuses) : base(statuses)
		{
			List<DeliverStatus> Success = new List<DeliverStatus>();
			List<DeliverStatus> Failure = new List<DeliverStatus>();

			foreach (var status in statuses)
				(status.Success ? Success : Failure).Add(status);

			this.Success = new ReadOnlyCollection<DeliverStatus>(Success);
			this.Failure = new ReadOnlyCollection<DeliverStatus>(Failure);
		}

		/// <summary>
		/// Indicate whether all are successful
		/// </summary>
		public bool AllSuccess => Failure.Count == 0;

		/// <summary>
		/// Successful receipt collection
		/// </summary>
		public IReadOnlyCollection<DeliverStatus> Success { get; private set; }

		/// <summary>
		/// Failed receipt collection
		/// </summary>
		public IReadOnlyCollection<DeliverStatus> Failure { get; private set; }

	}
}
