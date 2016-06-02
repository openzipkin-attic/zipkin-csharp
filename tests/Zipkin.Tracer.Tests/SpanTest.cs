using System;
using System.CodeDom;
using System.Net;
using Xunit;
using Zipkin.Thrift;

namespace Zipkin.Tracer.Tests
{
    public class SpanTest
    {
        [Fact]
        public void Span_should_serialize_properly()
        {
            var annotation = new Annotation("value", DateTime.Now, new IPEndPoint(IPAddress.Any, 2222));
            var traceId = new TraceHeader(123, 234, 345, true);
            var span = new Span(traceId, new IPEndPoint(IPAddress.Loopback, 1111), "service", "name");
            span.Record(annotation);

            var thrifted = span.ToThrift();
            var host = thrifted.Annotations[0].Host;

            Assert.Equal("service", host.ServiceName);
            Assert.True(thrifted.__isset.name, "Serialized span name not set");
            Assert.Equal("name", thrifted.Name);
            Assert.False(thrifted.__isset.binary_annotations, "Serialized span binary annotations should not be set");
            Assert.True(thrifted.__isset.trace_id, "Serialized span trace_id not set");
            Assert.Equal(123, thrifted.TraceId);
            Assert.True(thrifted.__isset.id, "Serialized span id not set");
            Assert.Equal(234, thrifted.Id);
            Assert.True(thrifted.__isset.parent_id, "Serialized span parent_id not set");
            Assert.Equal(345, thrifted.ParentId);
            Assert.True(thrifted.Debug);
        }
    }
}