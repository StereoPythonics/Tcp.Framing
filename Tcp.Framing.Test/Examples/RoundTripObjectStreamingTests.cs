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
public class TestNetworkStreamBuilder
{   
    public static TestNetworkStreamBuilder Instance { get {
        instance ??= new TestNetworkStreamBuilder();
        return instance;
    }}
    static TestNetworkStreamBuilder instance;
    static int runningPort = 5678;
    private TestNetworkStreamBuilder(){
        runningPort = 5678;
    }
    public static TestStreamPair GetTestStreamPair()
    {
        NetworkStream returnableListener = null;
        NetworkStream returnableClient;
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),runningPort);
        listener.Start();

        var getListenerStream = Task.Run(() => {
            TcpClient client = listener.AcceptTcpClient();
            returnableListener = client.GetStream();
        });

        using TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",runningPort);
        returnableClient = readClient.GetStream();
        getListenerStream.Wait();
        return new TestStreamPair(){ListenerStream = returnableListener, ClientStream = returnableClient};
    }  
    public class TestStreamPair
    {
        public NetworkStream ListenerStream;
        public NetworkStream ClientStream;
    }

}
