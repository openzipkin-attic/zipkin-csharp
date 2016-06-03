using System;
using System.Net;
using System.Threading;
using Zipkin;
using Zipkin.Tracer.Kafka;

namespace ZipkinExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            // make sure Zipkin with Scribe client is working
            //var collector = new HttpCollector(new Uri("http://localhost:9411/"));
            var collector = new KafkaCollector(KafkaSettings.Default);
            var traceId = new TraceHeader(traceId: (ulong)random.Next(), spanId: (ulong)random.Next());
            var span = new Span(traceId, new IPEndPoint(IPAddress.Loopback, 9000), "test-service");
            span.Record(Annotations.ClientSend(DateTime.UtcNow));
            Thread.Sleep(100);
            span.Record(Annotations.ServerReceive(DateTime.UtcNow));
            Thread.Sleep(100);
            span.Record(Annotations.ServerSend(DateTime.UtcNow));
            Thread.Sleep(100);
            span.Record(Annotations.ClientReceive(DateTime.UtcNow));

            collector.CollectAsync(span).Wait();
        }
    }
}
