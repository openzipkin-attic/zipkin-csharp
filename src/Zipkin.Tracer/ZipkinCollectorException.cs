using System;
using System.Runtime.Serialization;

namespace Zipkin
{
    [Serializable]
    public class ZipkinCollectorException : Exception
    {
        public ZipkinCollectorException()
        {
        }

        public ZipkinCollectorException(string message) : base(message)
        {
        }

        public ZipkinCollectorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ZipkinCollectorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}