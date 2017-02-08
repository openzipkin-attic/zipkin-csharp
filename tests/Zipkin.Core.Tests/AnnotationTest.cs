//-----------------------------------------------------------------------
// <copyright file="AnnotationTest.cs" company="Bazinga Technologies Inc.">
//     Copyright (C) 2016 Bazinga Technologies Inc.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Net;
using Xunit;
using Zipkin.Thrift;

namespace Zipkin.Tracer.Tests
{
    public class AnnotationTest
    {
        [Fact]
        public void Annotation_should_serialize_properly()
        {
            var annotation = new Annotation("value", new DateTime(2016, 10, 1), new IPEndPoint(IPAddress.Loopback, 1233));
            var thrifted = annotation.ToThrift();

            Assert.True(thrifted.__isset.host, "Serialized annotation host not set");
            Assert.Equal(16777343, thrifted.Host.Ipv4); // System.BitConverter.ToInt32(IPAddress.Loopback.GetAddressBytes(), 0)
            Assert.Equal(annotation.Endpoint.Port, thrifted.Host.Port);
            Assert.True(thrifted.__isset.value, "Serialized annotation value not set");
            Assert.Equal(annotation.Value, thrifted.Value);
            Assert.True(thrifted.__isset.timestamp, "Serialized annotation timestamp not set");
            Assert.Equal((annotation.Timestamp.Ticks - new DateTime(1970, 1, 1).Ticks) / AnnotationConstants.TicksPerMicosecond, thrifted.Timestamp);
        }
    }
}