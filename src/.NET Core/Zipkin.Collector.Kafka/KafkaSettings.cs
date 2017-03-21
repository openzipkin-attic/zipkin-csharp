//-----------------------------------------------------------------------
// <copyright file="KafkaSettings.cs" company="Bazinga Technologies Inc.">
//     Copyright (C) 2016 Bazinga Technologies Inc.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;

namespace Zipkin.Tracer.Kafka
{
#if !NETSTANDARD1_5 && !SILVERLIGHT
    [Serializable]
#endif
    public sealed class KafkaSettings
    {
        public static KafkaSettings Default => new KafkaSettings("zipkin", 20, 1000, new []{ "http://localhost:9092" });
        public static KafkaSettings Create(params Uri[] serverUris)
            => new KafkaSettings("zipkin", 20, 1000, serverUris.Select(x => x.ToString()).ToArray());

        public KafkaSettings(string zipkinTopic, int maxAsyncRequests, int maxMessageBuffer, string[] serverUris)
        {
            ZipkinTopic = zipkinTopic;
            ServerUris = serverUris;
            MaxAsyncRequests = maxAsyncRequests;
            MaxMessageBuffer = maxMessageBuffer;
        }

        public string[] ServerUris { get; }
        public int MaxAsyncRequests { get; }
        public int MaxMessageBuffer { get; }
        public string ZipkinTopic { get; }
    }
}