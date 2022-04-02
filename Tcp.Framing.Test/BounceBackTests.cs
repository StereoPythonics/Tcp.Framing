using System.Net;
using System.Net.Sockets;
using Xunit;
namespace Tcp.Framing.Test;

public class BounceBackTests
{
    [Trait("cat", "ext")]//Needs external application to chat with
    [Fact]
    public void Bounce1000Messages()
    {
        ExampleTestObject input = new ExampleTestObject() { ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice" };

        IObjectEnumerator<ExampleTestObject> receiveObjectStreamer = null;
        IObjectEnumerator<ExampleTestObject> sendObjectStreamer = null;

        IBlobSerializer<ExampleTestObject> serializer = new GZipedJsonSerializer<ExampleTestObject>();
        

        var senderSetUp = Task.Run(async () =>
        {
            using TcpClient readClient = new TcpClient();
            readClient.Connect("127.0.0.1", 4456);
            using NetworkStream clientNetworkStream = readClient.GetStream();
            sendObjectStreamer = new ObjectStreamer<ExampleTestObject>(
                clientNetworkStream,
                serializer,
                injectedCancellationTokenGenerator: () => new CancellationTokenSource().Token
                );
            await sendObjectStreamer.WriteAsyncEnumerable(AsyncEnumerable.Range(0, 100000).Select(i => input with {ExampleInt = i}));
        });
        
        var outputItems = new List<ExampleTestObject>();
        var receiverSetUp = Task.Run(async () =>
        {
            using TcpClient readClient = new TcpClient();
            readClient.Connect("127.0.0.1", 4457);
            using NetworkStream clientNetworkStream = readClient.GetStream();
            receiveObjectStreamer = new ObjectStreamer<ExampleTestObject>(
                clientNetworkStream, 
                serializer,
                injectedCancellationTokenGenerator: () => new CancellationTokenSource().Token);
            outputItems = await receiveObjectStreamer.ReadAsyncEnumerable().Take(100000).ToListAsync();
        });

        Task.WaitAll(receiverSetUp, senderSetUp);
        Assert.Equal(100000, outputItems.Count);
        Assert.True(outputItems.Select((m,i) => m.ExampleInt == i).All(b => b));
    }
}

