using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GoFastDFS.Client
{
	internal delegate void EnterNewEnvelopHandler();

	/// <summary>
	/// Represents a thread-safe first in-first out (FIFO) collection which is for <see cref="Envelope"/>.
	/// </summary>
	internal sealed class EnvelopeQueue
	{
		private readonly ConcurrentQueue<Envelope> EnvelopeContainer = new ConcurrentQueue<Envelope>();
		private readonly ConcurrentQueue<Envelope> PriorityContainer = new ConcurrentQueue<Envelope>();

		/// <summary>
		/// This extra event is triggered when the queue is saved to a new object.
		/// </summary>
		internal event EnterNewEnvelopHandler EnterNewEnvelopCallback = new EnterNewEnvelopHandler(() => { });

		/// <summary>
		/// Gets the number of elements contained in <see cref="EnvelopeQueue"/>
		/// </summary>
		internal int Count => EnvelopeContainer.Count + PriorityContainer.Count;

		/// <summary>
		/// Cache the  <see cref="Envelope"/> into the <see cref="EnvelopeQueue"/>
		/// </summary>
		/// <param name="envelope">Cache object</param>
		/// <param name="InFront">Mark if enter the priority queue</param>
		internal void Enqueue(Envelope envelope, bool InFront = false)
		{
			(InFront ? PriorityContainer : EnvelopeContainer).Enqueue(envelope);
			EnterNewEnvelopCallback.Invoke();
		}

		/// <summary>
		/// Cache the Several  <see cref="Envelope"/>s into the <see cref="EnvelopeQueue"/>
		/// </summary>
		/// <param name="envelopes">Collection of cached objects</param>
		/// <param name="InFront">Mark if enter the priority queue</param>
		internal void Enqueue(IEnumerable<Envelope> envelopes, bool InFront = false)
		{
			var container = InFront ? PriorityContainer : EnvelopeContainer;
			foreach (Envelope envelope in envelopes) container.Enqueue(envelope);
			EnterNewEnvelopCallback.Invoke();
		}

		/// <summary>
		/// Try to take <see cref="Envelope"/> ​​from the queue. Priority will be given to the value from the priority queue.
		/// </summary>
		/// <returns>The <see cref="Envelope"/> in the queue or null if there is no value in the queue</returns>
		internal Envelope Dequeue()
		{
			if (PriorityContainer.TryDequeue(out Envelope envelop)) return envelop;
			else if (EnvelopeContainer.TryDequeue(out envelop)) return envelop;
			return envelop;
		}

		
	}
}
