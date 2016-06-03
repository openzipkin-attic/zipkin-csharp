using System;
using System.Threading.Tasks;

namespace Zipkin
{
    /// <summary>
    /// An interface used to communicate with one of the Zipkin span receivers.
    /// </summary>
    public interface ISpanCollector
    {
        /// <summary>
        /// Asynchronously sends a series of <see cref="Span"/>s to Zipkin receiver.
        /// </summary>
        Task CollectAsync(params Span[] spans);
    }
}