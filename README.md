![alt tag](https://github.com/stereopythonics/Tcp.Framing/tcp.framing.logo.png)
# Tcp.Framing
A simple, opinionated, yet extensible library for pushing stuff over TCP streams.

## At a glance

Provided you already have your network streams, sending objects over a shared network stream is trivial.

On your server:
```csharp
IObjectStreamer<ExampleObject> serverObjectStreamer = new ObjectStreamer<ExampleObject>(serverNetworkStream);
serverObjectStreamer.WriteObject(inputObject);
```

On your client:
```csharp
IObjectStreamer<ExampleObject> ClientObjectStreamer = new ObjectStreamer<ExampleObject>(clientNetworkStream);
outputObject = clientObjectStreamer.ReadObject();
```

## Example TcpListener/TcpClient configuration.

You probably don't have network streams to work with yet.
Not to worry! Here's a barebones example that demonstrates how this library integrates with the System.Net.Sockets objects.

This example is chatting with itself over localhost, but the TcpListener / TcpClient can be split between machines for real world applications.

```csharp
using System.Net.Sockets;
using System.Net;
using Xunit;

namespace Tcp.Framing.Test;
public class TcpListenerClientExample
{
    [Fact]
    public void PassObjectThroughTcpSocket()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};

        //Setup serverside listener
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),3456);
        listener.Start();

        //Handle incomming connection on background thread
        Task.Run(() => {
            using TcpClient client = listener.AcceptTcpClient(); //blocks waiting for client connection
            using NetworkStream serverNetworkStream = client.GetStream();

            IBlockingObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(serverNetworkStream);
            serverObjectStreamer.WriteObject(input);
        });

        //Setup client side connection
        using TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",3456);
        using NetworkStream clientNetworkStream = readClient.GetStream();

        //Confirm object passed through connection
        IBlockingObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(clientNetworkStream);
        Assert.Equal(input,clientObjectStreamer.ReadObject());
    }
}
```

Documentation lies (especially mine), but unit tests don't! Find more working examples under Tcp.Framing.Test/Examples

# The Details

## Contents
The above should be enough to get you started, but if you're interested in the details here's a summary of what's included here:

- [Object Serialization](#object-serialization)
    - System.Text.Json
    - Compression by default
    - Advice on rolling your own
- [Framing Approach](#framing-approach)
    - Length Prefixing
    - Start/End Markers
- [Conversational TCP](#conversational-tcp)
    - Overwhelming your target
    - Seeking Acknowledgement
    - Calling It Quits
- [Screw the streams! I just want bytes!](#screw-the-streams-i-just-want-bytes)
- [Enumeration](#enumeration)
- [TPL Dataflow](#tpl-dataflow)
- [Future Plans](#future-plans)
    - Handling drop outs

## Object Serialization

### System.Text.Json

Your objects need to end up as bytes somehow, and while inefficient, utf8 json is a good universal starting point.

This library makes use of System.Text.Json to handle this, but does so through the ```IBlobSerializer``` interface to users to easily swap in their own implementiation.

### Compression by default
By default, the ```GZipedJsonSerializer``` is used to convert objects to blobs. If you are dealing with many small objects, GZip compression may not be beneficial, and the plaintext ```UTF8JsonSerializer``` can be injected instead. 

### Advice on rolling your own

The generic ```IBlobSerializer``` interface makes it easy for you to create and use your own serialization implementation.
```csharp
public interface IBlobSerializer<T>
{
    byte[] Serialize(T inputObject);
    T Deserialize(ReadOnlySpan<byte> inputBytes);
}
```

You can inject your custom serializer into an ```ObjectStreamer``` at construction.

```csharp
var streamer = new ObjectStreamer<ExampleTestObject>(serverNetworkStream, new YourCustomSerializer());
```

## Framing Approach
It's hard to work with raw network streams, you can request groups of bytes arbitrarily, but there's no inbuilt mechanism for grouping those bytes as descrete messages / objects.
Framing allows us to wrap an arbitrary set of bytes with identifying information enabling structured retrieval.

### Length Prefixing
Specifically Tcp.Framing uses length prefixing to frame byte groups. Length prefixing uses some bytes before a message body to describe the expected message body length. This allows a reader to first read the prefix bytes to get the body length, and then use this length to request the whole message body in one go.

As a simplified and hypothetical example, we could use length prefixing to send integers in a stream of arabic numerals.

For example ```123456789123456789123``` would translate to the messages:

```(1)[2], (3)[456], (7)[8912345], (6)[789123]``` where ```(length prefix)[body]``` represents each prefix and message body.

In Tcp.Framing, a standard prefix of 4 bytes is used to encode the message body length, these bytes are parsed as a signed int giving a maximum message body length of int.maxValue (~2.1GB). If your messages are larger than this, then you have bigger problems. 

As a convention, this signed int is encoded/decoded as little-endian regardless of system endianness to ensure cross-device compatibility.

for example a 5 byte message body ```A1 B2 C3 D4 E5``` would be framed as:

```00 00 00 05 A1 B2 C3 D4 E5``` where ```00 00 00 05``` is the 4 byte length prefix.

### Start/End Markers

In addition to length prefixing, TCP.Framing also makes use of message start and end markers for error detection purposes. (unbounded message framing using bounding markers is not supported)

Even with TCP, bytes can go missing if one machine overwhelms another with data. Tcp.Framing uses arbitrary byte patterns to mark the expected start and end of a frame, checks for these when reading messages from a stream, and will throw if the expected markers are missing.

For example, with arbitrary frame markers ```AA BB CC DD```(start) and ```DD CC BB AA```(end) the prior 5 byte message body ```A1 B2 C3 D4 E5``` would be fully framed as:

```AA BB CC DD``` ```00 00 00 05``` ```A1 B2 C3 D4 E5``` ```DD BB CC DD```

## Conversational TCP

### Overwhelming your target

The TCP protocol does a great job of ensuring all your bytes make it over the wire. However in some scenarios, a device can eventually overwhelm another if stream write-rate is greater than the read-rate. There's a limit to how much data can buffer between the sender and reciever before something gives, and rather than expose you to the gritty details of tuning TCP Socket Buffer Sizes, Windows and understanding Bandwidth-Delay Products, this library handles this concern through bidirectional acknowedgement at the code level.

### Seeking Acknowledgement

To prevent an overflowing buildup of in-flight messages, TCP.Framing uses bi-directional communication between the message writing and message reading code to ensure they're running at roughly the same rate. This is a lot like the interaction of reciting a number over the phone:

- ('Ok my number is 06789',"yep",'123', "yep", '456', "ok thanks")

The code for this is simple and can be seen in ```AcknowledgedAsyncBlobStreamer.cs```

```csharp
public async Task WriteBlob(byte[] inputBlob, CancellationToken cancellationToken = default)
{
    await _streamWriter.WriteBlobAsFrame(inputBlob, _stream, cancellationToken);
    await WaitForBlobAcknowledgement(cancellationToken);
}
public async Task<byte[]> ReadBlob(CancellationToken cancellationToken = default)
{
    byte[] returnable = await _streamWriter.ReadFrameAsBlob(_stream, cancellationToken);
    await SendBlobAcknowledgement(cancellationToken);
    return returnable;
}
```
### Calling it Quits

What happens if you never receive acknowledgement, or are waiting for a message that never comes? Something may have gone awry with your connection, or the code at the other end. It's important to know when to give up.

With synchronous calls to ```NetworkStream.Read``` or ```NetworkStream.Write``` this timeout is defined through ```NetworkStream.ReadTimeout``` or ```NetworkStream.WriteTimeout```. However with the```NetworkStream.[Read/Write]Async``` methods used by Tcp.Framing, there is no default timeout behaviour.

High level Tcp.Framing classes such as ```ObjectStreamer<T>``` enforce a default ```30s``` timeout for read/write methods.  But an alternative cancellation approach me be injected via construction

```csharp
public ObjectStreamer(Stream stream, IBlobSerializer<T> serializer = null, IBlobStreamer blobStreamer = null, Func<CancellationToken> injectedCancellationTokenGenerator = null)
```

A custom construction with shorter (1.5s) timeout might look like:

```csharp
ObjectStreamer os = new ObjectStreamer(
        someNetworkStream,
        injectedCancellationTokenGenerator: () => new CancellationTokenSource(1500).Token
    );
```

You can also specify specific tokens for individual methods from ```IObjectStreamer``` and ```IObjectEnumerator``` where specific timeouts are desired.

```csharp
[Fact]
public async Task ConfirmAsyncReadAcceptsCancellation()
{
    var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1244);
    IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
    //Notice no object writer is configured, the reader will be waiting forever!

    await Assert.ThrowsAsync<OperationCanceledException>(async () => 
        await clientObjectStreamer.ReadObjectAsync(new CancellationTokenSource(500).Token)
    );
}
```

## Screw the streams! I just want bytes!

Ok, so streams aside, you just want access to the framing methods. That's fine.

Framing Approach classes like ```LPrefixAndMarkersBlobFramer.cs``` implement the ```IBlobFramer``` interface so you can get byte arrays if you want.

```csharp
public interface IBlobFramer
{
    Task<byte[]> FrameBlob(byte[] bytes);
    Task<byte[]> UnframeBlob(byte[] bytes);
}
```

In many cases this will wrap the ```IFramedBlobStreamWriter``` methods, using a temorary MemoryStream to get your bytes.

```csharp
public async Task<byte[]> FrameBlob(byte[] input)
{
    using MemoryStream ms = new MemoryStream();
    await FrameBlobAsync(input,ms);
    return ms.ToArray();
}
public async Task<byte[]> UnframeBlob(byte[] input)
{
    using MemoryStream ms = new MemoryStream(input.ToArray());
    return await UnframeBlobAsync(ms);
}
```
## Enumeration

LINQ enthusiasts will be pleased to know that the ```ObjectStreamer``` supports enumerable streaming via the ```IObjectEnumerator``` interface. It can handle both both sending and receiving enumerables.

```csharp
public interface IObjectEnumerator<T>
{
    IAsyncEnumerable<T> ReadAsyncEnumerable(CancellationToken cancellationToken = default);
    Task WriteAsyncEnumerable(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default);
    Task WriteEnumerable(IEnumerable<T> source, CancellationToken cancellationToken = default);
}
```

It is critical to note that when pipelining from an ```IAsyncEnumerable<T>```, the enumerable is unbounded. And pipelines like:

```csharp
var output = await clientObjectStreamer.ReadAsyncEnumerable().Select(a => a.ExampleInt).ToListAsync(); 
```
will not resolve, and will meet the default timout specified configured for ```ObjectStreamer```.

To avoid this scenario, you can limit the enumeration with Take(),

```csharp
var output = await clientObjectStreamer.ReadAsyncEnumerable().Take(100).Select(a => a.ExampleInt).ToListAsync(); 
```

Or if you're intent to wrestle with an unbounded enumerable (I hope you know what you're going) that you can override the default cancellation:

```csharp
var output = await clientObjectStreamer.ReadAsyncEnumerable(new CancellationTokenSource().Token).Select(a => a.ExampleInt).ToListAsync(); //this example will hang
```

## TPL Dataflow
TPL dataflow is magic and a great way of dealing with message flows asynchronously. Tcp.Framing.Dataflow exposes the following Sink/Source interfaces for message pipelining.

For Sending:
```csharp
public interface ISink<T>
{
    BufferBlock<T> SinkBlock { get; }
}
```

For Receiving:
```csharp
public interface ISource<T>
{
    BroadcastBlock<T> SourceBlock { get; }
}
```

These come with piecemeal and batched transport implementations ```Object[Sink/Source]``` and ```BatchedObject[Sink/Source]``` that only require a stream to construct.

```csharp
[Fact]
public void ConfirmRoundTrip()
{
    var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1237);
    ISource<TestObject> testSource = new ObjectSource<TestObject>(streamPair.ListenerStream);
    ISink<TestObject> testSink = new ObjectSink<TestObject>(streamPair.ClientStream);
    ConfirmGoodTransmisison(testSource,testSink);
}
```


## Future Plans


### Handling drop outs
There's zero provision in this code for real world connectivity issues and dropouts. There needs to be!



