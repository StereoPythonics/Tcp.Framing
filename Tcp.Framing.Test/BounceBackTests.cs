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
        var bytes = new byte[1000000];
        new Random().NextBytes(bytes);
        ExampleTestObject input = new ExampleTestObject() { ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = Convert.ToBase64String(bytes) };

        IObjectEnumerator<ExampleTestObject> receiveObjectStreamer = null;
        IObjectEnumerator<ExampleTestObject> sendObjectStreamer = null;

        IBlobSerializer<ExampleTestObject> serializer = new GZipedJsonSerializer<ExampleTestObject>();
        string ip = "192.168.0.11";
        int messageCountTarget = 1000;

        var senderSetUp = Task.Run(async () =>
        {
            using TcpClient readClient = new TcpClient();
            readClient.Connect(ip, 4456);
            using NetworkStream clientNetworkStream = readClient.GetStream();
            sendObjectStreamer = new ObjectStreamer<ExampleTestObject>(
                clientNetworkStream,
                serializer,
                injectedCancellationTokenGenerator: () => new CancellationTokenSource().Token
                );
            await sendObjectStreamer.WriteAsyncEnumerable(AsyncEnumerable.Range(0, messageCountTarget).Select(i => input with {ExampleInt = i}));
        });
        
        var outputItems = new List<ExampleTestObject>();
        var receiverSetUp = Task.Run(async () =>
        {
            using TcpClient readClient = new TcpClient();
            readClient.Connect(ip, 4457);
            using NetworkStream clientNetworkStream = readClient.GetStream();
            receiveObjectStreamer = new ObjectStreamer<ExampleTestObject>(
                clientNetworkStream, 
                serializer,
                injectedCancellationTokenGenerator: () => new CancellationTokenSource().Token);
            outputItems = await receiveObjectStreamer.ReadAsyncEnumerable().Take(messageCountTarget).ToListAsync();
        });

        Task.WaitAll(receiverSetUp, senderSetUp);
        Assert.Equal(messageCountTarget, outputItems.Count);
        Assert.True(outputItems.Select((m,i) => m.ExampleInt == i).All(b => b));
    }
}

