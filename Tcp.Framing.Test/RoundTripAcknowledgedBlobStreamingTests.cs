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

        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1234);

        Task.Run(() => {
            IBlobStreamer inputBlobStreamer = new AcknowledgedBlobStreamer(streamPair.ListenerStream,streamWriter);
            inputBlobStreamer.WriteBlob(testInputBlob);
        });

        IBlobStreamer outputBlobStreamer = new AcknowledgedBlobStreamer(streamPair.ClientStream,streamWriter);
        testOutputBlob = outputBlobStreamer.ReadBlob();

        Assert.True(testInputBlob.SequenceEqual(testOutputBlob));
    }
}
