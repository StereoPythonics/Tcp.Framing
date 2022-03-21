# Tcp.Framing
A simple, opinionated yet extensible library for pushing stuff over TCP streams.

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
public class RoundTripObjectStreamingTests
{
    [Fact]
    public void TestSingleObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};

        
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),3456);
        listener.Start();

        //Handle incomming connection on background thread
        Task.Run(() => {
            using TcpClient client = listener.AcceptTcpClient(); //blocks waiting for client connection
            using NetworkStream serverNetworkStream = client.GetStream();

            IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(serverNetworkStream);
            serverObjectStreamer.WriteObject(input);
        });

        using TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",3456);
        using NetworkStream clientNetworkStream = readClient.GetStream();

        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(clientNetworkStream);
        Assert.Equal(input,clientObjectStreamer.ReadObject());
    }
}
```

Documentation lies (especially mine), but unit tests don't! Find more working examples under Tcp.Framing.Test/Examples

# The Details

## Contents
The above should be enough to get you started, but if you're interested in the details here's a summary of what's included here:'

- Object Serialization
    - System.Text.Json
    - Advice on rolling your own
- Framing Approach
    - Length Prefixing
    - Start/End Markers
- Conversational TCP
    - Overwhelming your target
    - Seeking Acknowledgement
- Screw the streams! I just want bytes!
- Future Plans
    - TPL Dataflow
    - Enumeration
    - Async
    - Handling drop outs

## Object Serialization

### System.Text.Json

Your objects need to end up as bytes somehow, and while inefficient, utf8 json is a good universal starting point.

This library makes use of System.Text.Json to handle this, but does so through the ```IBlobSerializer``` interface to users to easily swap in their own implementiation.

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
Framing allows us to wrap an arbitrary set of bytes with identifying information enabling structured retrieval

### Length Prefixing
Specifically Tcp.Framing makes use of length prefixing to frame byte groups. Length prefixing uses some bytes befor a message to describe the total message body length, allowing a reader to first read the a set prefix bytes to get the body length, and then use the length to request the whole message body in one go.

As a simplified and hypothetical example, we could use length prefixing to send integers in a stream of arabic numerals.

For example ```12345678912345678912``` would translate to the messages:

```(1)[2], (3)[456], (7)[8912345], (6)[78912]``` where ```(length prefix)[body]``` represents each prefix and message body.

In TCP.Framing, 4 bytes are dedicated to encoding the body length, these are parsed as a signed int giving a maximum message body length of int.maxValue (~2.1GB). if your messages are larger than this, then you have bigger problems. 

As a convention, this int is encoded/decoded as little endian regardless of system endianess to ensure cross-device compatibility.

for example a 5 byte message body ```A1 B2 C3 D4 E5``` would have be transmitted as:

```00 00 00 05 A1 B2 C3 D4 E5``` where ```00 00 00 05``` is the 4 byte length prefix.

### Start/End Markers

In addition to length prefixing, TCP.Framing also makes use of message start and end markers for error detection purposes only (unbounded messages are not supported).

Even with TCP, bytes can go missing if one machine overwhelms the other. Tcp.Framing uses arbitrary byte patterns to mark the expected start and end of a frame, checks for these when reading messages from a stream, and will throw if the expected markers are missing.

For example, with arbitrary frame markers ```AA BB CC DD```(start) and ```DD CC BB AA```(end) the prior 5 byte message body ```A1 B2 C3 D4 E5``` would be fully framed as:

```AA BB CC DD``` ```00 00 00 05``` ```A1 B2 C3 D4 E5``` ```DD BB CC DD```

## Conversational TCP

### Overwhelming your target

TCP does a great job of ensuring all your bytes make it over the wire. However in some scenarios, a device can overwhelm another if stream write-rate > read-rate. There's a limit to how much data can buffer between the sender and reciever before something gives, and rather than expose you to the gritty details around Socket Buffer Sizes, Windows and Bandwidth-Delay Products 

### Seeking Acknowledgement

To prevent an overflowing buildup of in-flight messages, TCP.Framing uses bi-directional communication between the message writing and message reading code to ensure they're running at the same rate. This is a lot like the interaction of reciting a number over the phone:
 ('Ok it's 06789',"yep",'123', "yep", '456', "ok thanks")

You code for this simple and can be seen in ```AcknowledgedBlobStreamer.cs```

```csharp
    

    public void WriteBlob(ReadOnlySpan<byte> inputBlob)
    {
        _streamWriter.WriteBlobAsFrame(inputBlob, _stream);
        WaitForBlobAcknowledgement(); //blocks until an acknowledgement is recieved
    }

    public byte[] ReadBlob()
    {
        byte[] returnable = _streamWriter.ReadFrameAsBlob(_stream);
        SendBlobAcknowledgement();
        return returnable;
    }
```

## Screw the streams! I just want bytes!

Ok, so streams aside, you just want access to the framing methods. That's fine.

Framing Approach classes like ```LPrefixAndMarkersBlobFramer.cs``` implement the IBlobFramer interface so you can get byte arrays if you want.

```csharp
public interface IBlobFramer
{
    byte[] FrameBlob(ReadOnlySpan<byte> bytes);
    byte[] UnframeBlob(ReadOnlySpan<byte> bytes);
}
```

## Future Plans
The following are a work in progress. Async and Enumeration probably belong in this package. TPL dataflow integration will hook into some other dotnet packages and should probably have it's own TCP.Framing.Dataflow package.
### TPL Dataflow
### Enumeration

I love working with LINQ, with the new batching methods it would be really cool to have something like
```csharp
clientObjectStreamer.EnumerateObjects();
```
that might yield transmitted objecs into an unbounded IEnumerable<exampleObject> that I can utilize through LINQ pipelining.
### Async
It's crazy not to have async methods for interacting with IO, Expect something like 
```csharp
clientObjectStreamer.ReadObjectAsync();
```
in the future!

### Handling drop outs
There's zero provision in this code for real world connectivity issues and dropouts. There needs to be!



