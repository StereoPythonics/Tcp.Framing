using System.Net.Sockets;
using System.Net;
using System.Text;
using Xunit;

namespace Tcp.Framing.Test;

public class RoundTripAcknowledgedBlobStreamingTests
{
    [Fact]
    public void TestRoundTripOverTcpStream()
    {
        byte[] testInputBlob = Encoding.ASCII.GetBytes("Test Blob");
        byte[] testOutputBlob = new byte[0];
        IFramedBlobStreamWriter streamWriter = new LPrefixAndMarkersBlobFramer();

        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),1234);
        listener.Start();

        Task.Run(() => {
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream ns = client.GetStream();
            IBlobStreamer inputBlobStreamer = new AcknowledgedBlobStreamer(ns,streamWriter);
            inputBlobStreamer.WriteBlob(testInputBlob);
        });

        TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",1234);
        NetworkStream ns = readClient.GetStream();

        IBlobStreamer outputBlobStreamer = new AcknowledgedBlobStreamer(ns,streamWriter);
        testOutputBlob = outputBlobStreamer.ReadBlob();


        Assert.True(testInputBlob.SequenceEqual(testOutputBlob));
    }
}
