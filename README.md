# Zipkin.Tracer

A minimalistic .NET client library for Twitter [Zipkin](http://zipkin.io/) tracing.

It provides a handy set of Zipkin primitives such as spans, annotations, binary annotations to Zipkin receiver through chosen protocol, using Thrift encoding for efficiency. *For now only HTTP is available, Kafka support is planned.*

It does **NOT** keep any kind of logical trace context for you. This way it avoids any dependencies on things like HttpContext or building complex abstractions. It also does **NOT** try to introduce any retry or failure handling policies. All of this + extremely small API makes it a great choice as low level library to be used by custom higher level plugins.

## Example

```csharp
// create a span
var trace = new TraceHeader(traceId: (ulong)random.Next(), spanId: (ulong)random.Next());
var span = new Span(traceId, new IPEndPoint(serviceIp, servicePort), "test-service");

span.Record(Annotations.ServerReceive(DateTime.UtcNow));
// ... handle a RPC request
span.Record(Annotations.ClientReceive(DateTime.UtcNow));

// send data to to zipkin
var succeed = await collector.CollectAsync(span);
```

## API

#### `ISpanCollector`

An interface used to communicate with one of the Zipkin span receivers.

- `Task<bool> CollectAsync(params Span[] spans)` - Asynchronously sends a series of spans and eventually returns a flag determining if they were received successfully.

Span collector has following implementations:

- `HttpCollector` - A collector sending all data to zipkin endpoint using HTTP protocol. Spans are encoded using Thrift encoding.
- `DebugCollector` - Debug collector used for printing spans into provided output.

#### `Span`

A set of annotations and binary annotations that corresponds to a particular RPC.

- `TraceHeader TraceHeader` - Trace header containing are identifiers necessary to locate current span.
- `string ServiceName` - Name of the service displayed by Zipkin UI.
- `ICollection<Annotation> Annotations` - Collection of annotations recorder withing current span time frame.
- `ICollection<BinaryAnnotation> BinaryAnnotations` - Collection of binary annotations used to attach additional metadata with the span itself.
- `IPEndPoint Endpoint` - Endpoint, target span's service is listening on.
- `void Record(Annotation annotation)` - Records an annotation within current span. Also sets it's endpoint if it was not set previously.
- `void Record(BinaryAnnotation binaryAnnotation)` - Records a binary annotation within current span. Also sets it's endpoint if it was not set previously.

#### `TraceHeader`

A structure containing all of the data necessary to identify span and it's trace among others.

- `ulong TraceId` - The overall ID of the trace. Every span in a trace will share this ID.
- `ulong SpanId` - The ID for a particular span. This may or may not be the same as the trace id.
- `ulong? ParentId` - This is an optional ID that will only be present on child spans. That is the span without a parent id is considered the root of the trace.
- `bool IsDebug` - Marks current span with debug flag.
- `TraceHeader Child(ulong childId)` - Creates a trace header for the new span being a child of the span identified by current trace header.

#### `Annotation`

An Annotation is used to record an occurance in time.

- `DateTime Timestamp` - Timestamp marking the occurrence of an event.
- `string Value` - Value holding an information about the annotation.
- `IPEndPoint Endpoint` - Service endpoint.

#### `BinaryAnnotation`

Special annotation without time component. They can carry extra information i.e. when calling an HTTP service &rArr; URI of the call.

- `string Key` - Key identifier of binnary annotation.
- `byte[] Value` - Binary annotation's value as binary.
- `AnnotationType AnnotationType` - Enum identifying type of value stored inside Value field.
- `IPEndPoint Endpoint` - Service endpoint.

#### `Annotations`

Static class containing set of constructors for some of the annotations (also binary annotations) recognized by Zipkin itself i.e: `ServerSend`, `ClientSend`, `ServerReceive` or `ClientReceive`.

#### `AnnotationConstants`

Static class with set of values, that could be used as `Annotation` values of `BinaryAnnotation` keys.
