//-----------------------------------------------------------------------
// <copyright file="KafkaCollector.cs" company="Bazinga Technologies Inc.">
//     Copyright (C) 2016 Bazinga Technologies Inc.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using Zipkin.Thrift;

namespace Zipkin.Tracer.Kafka
{
    public class KafkaCollector : ISpanCollector, IDisposable
    {
        private readonly KafkaSettings _settings;
        private readonly Producer _producer;

        public KafkaCollector(KafkaSettings settings)
        {
            _settings = settings;
            var options = new KafkaOptions(settings.ServerUris.Select(x => new Uri(x)).ToArray());
            var router = new BrokerRouter(options);
            _producer = new Producer(router, settings.MaxAsyncRequests, settings.MaxMessageBuffer);
        }

        public async Task CollectAsync(params Span[] spans)
        {
            using (var stream = new MemoryStream())
            {
                ThriftSpanSerializer.WriteSpans(spans, stream);
                stream.Position = 0;

                var message = new Message
                {
                    Value = stream.ToArray()
                };

                var result = await _producer.SendMessageAsync(_settings.ZipkinTopic, new[] { message });
                var res = result.First();
                if (res.Error != 0)
                {
                    throw new ZipkinCollectorException($"An error (code: {res.Error}) occurred while sending trace data to zipkin-kafka");
                }
            }
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
