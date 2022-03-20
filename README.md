# Tcp.Framing
A simple library for pushing stuff over TCP streams.

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
- I just want bytes!
    - byte[] methods
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
Specifically TCP.Framing makes use of length prefixing to frame byte groups. Length prefixing uses the start of a message to describe the total message length, allowsing a reader to first read the prefix to get the length, and then use the length to request the whole message in one go.



## Conversational TCP

### Overwhelming your target
### Seeking Acknowledgement

## I just want bytes!
### byte[] methods
## Future Plans
### TPL Dataflow
### Enumeration
### Async
### Handling drop outs



