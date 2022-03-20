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

    [Fact]
    public void TestMultiObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};

        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),2345);
        listener.Start();

        Task.Run(() => {
            using TcpClient client = listener.AcceptTcpClient();
            using NetworkStream serverNetworkStream = client.GetStream();

            IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(serverNetworkStream);
            Enumerable.Range(0,100).ToList().ForEach(i => serverObjectStreamer.WriteObject(input));
        });

        using TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",2345);
        using NetworkStream clientNetworkStream = readClient.GetStream();

        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(clientNetworkStream);
        Enumerable.Range(0,100).ToList().ForEach(i => Assert.True(input.Equals(clientObjectStreamer.ReadObject())));
    }
}
