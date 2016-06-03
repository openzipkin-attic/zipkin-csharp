using System;
using System.Threading.Tasks;

namespace Zipkin.Tracer.Kafka
{
    public class KafkaCollector : ISpanCollector
    {
        public Task CollectAsync(params Span[] spans)
        {
            throw new NotImplementedException();
        }
    }
}
