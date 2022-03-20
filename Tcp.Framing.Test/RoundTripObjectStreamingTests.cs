using System.Net.Sockets;
using System.Net;
using Xunit;

namespace Tcp.Framing.Test;

public class RoundTripObjectStreamingTests
{
    public record ExampleObject{
        public string ExampleString {get;set;}
        public double ExampleDouble {get;set;}
        public int ExampleInt {get;set;}
    }
    [Fact]
    public void TestObjectStreamingRoundTrip()
    {
        ExampleObject input = new ExampleObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        ExampleObject output = null;

        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),2345);
        listener.Start();

        Task.Run(() => {
            using TcpClient client = listener.AcceptTcpClient();
            using NetworkStream ns = client.GetStream();
            IObjectStreamer<ExampleObject> inputObjectStreamer = new ObjectStreamer<ExampleObject>(ns);
            Enumerable.Range(0,100).ToList().ForEach(i => inputObjectStreamer.WriteObject(input));
        });

        using TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",2345);
        using NetworkStream ns = readClient.GetStream();

        IObjectStreamer<ExampleObject> outputObjectStreamer = new ObjectStreamer<ExampleObject>(ns);
        Enumerable.Range(0,100).ToList().ForEach(i => output = outputObjectStreamer.ReadObject());

        Assert.True(input.Equals(output));
    }
    

}
