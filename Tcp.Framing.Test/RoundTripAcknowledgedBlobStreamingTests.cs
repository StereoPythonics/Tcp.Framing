using System.Net.Sockets;
using System.Net;
using System.Text;
using Xunit;

namespace Tcp.Framing.Test;

public class RoundTripAcknowledgedBlobStreamingTests
{
    [Fact]
    public async Task TestRoundTripOverTcpStream()
    {
        byte[] testInputBlob = Encoding.ASCII.GetBytes("Test Blob");
        byte[] testOutputBlob = new byte[0];
        LPrefixAndMarkersBlobFramer streamWriter = new LPrefixAndMarkersBlobFramer();

        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1234);

        Task receive = Task.Run(async () => {
            IBlobStreamer inputBlobStreamer = new AcknowledgedAsyncBlobStreamer(streamPair.ListenerStream,streamWriter);
            await inputBlobStreamer.WriteBlob(testInputBlob);
        });

        IBlobStreamer outputBlobStreamer = new AcknowledgedAsyncBlobStreamer(streamPair.ClientStream,streamWriter);
        testOutputBlob = await outputBlobStreamer.ReadBlob();
        await receive;
        Assert.True(testInputBlob.SequenceEqual(testOutputBlob));
    }
}
