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
        /// Asynchronously sends a series of <see cref="Span"/>s and
        /// eventually returns a flag determining if they were received successfully.
        /// </summary>
        Task<bool> CollectAsync(params Span[] spans);
    }
}