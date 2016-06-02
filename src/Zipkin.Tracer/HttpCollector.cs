using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;
using Zipkin.Thrift;

namespace Zipkin
{
    /// <summary>
    /// A collector sending all data to zipkin endpoint using HTTP protocol.
    /// Spans are encoded using Thrift format.
    /// </summary>
    public class HttpCollector : ISpanCollector
    {
        private Uri _url;

        public HttpCollector(Uri url)
        {
            _url = url;
        }

        public HttpCollector() : this(new Uri("http://localhost:9411/api/v1/spans"))
        {
        }

        public async Task<bool> CollectAsync(params Span[] spans)
        {
            var serialized = SerializeSpans(spans);

            var request = WebRequest.CreateHttp(_url);
            request.Method = "POST";
            request.ContentType = "application/x-thrift";
            request.ContentLength = serialized.Length;
            File.WriteAllBytes("ok", serialized);
            using (var stream = await request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(serialized, 0, serialized.Length);
                await stream.FlushAsync();
            }
            using (var reply = (HttpWebResponse)await request.GetResponseAsync())
            {
                return reply.StatusCode == HttpStatusCode.Accepted;
            }
        }

        private static byte[] SerializeSpans(Span[] spans)
        {
            using (var buffer = new TMemoryBuffer())
            using (var protocol = new TBinaryProtocol(buffer))
            {
                protocol.WriteListBegin(new TList(TType.Struct, spans.Length));
                foreach (var span in spans)
                {
                    var thrift = span.ToThrift();
                    thrift.Write(protocol);
                }
                protocol.WriteListEnd();
                return buffer.GetBuffer();
            }
        }
    }
}