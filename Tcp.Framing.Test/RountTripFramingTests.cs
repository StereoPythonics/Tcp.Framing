using Xunit;
using System.Text;
namespace Tcp.Framing.Test;
public class RoundTripFramingTests
{
    [Fact]
    public async Task ConfirmRountTripStreamSuccess()
    {
        IFramedBlobStreamWriter blobStream = new LPrefixAndMarkersBlobFramer();
        using MemoryStream ms = new MemoryStream();
        byte[] inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        await blobStream.WriteBlobAsFrame(inputBytes,ms);
        ms.Seek(0,SeekOrigin.Begin);

        byte[] outputBytes = await blobStream.ReadFrameAsBlob(ms);

        Assert.True(inputBytes.SequenceEqual(outputBytes));
    }

    [Fact]
    public async Task ConfirmMissingStartFailure()
    {
        IBlobFramer blobFramer = new LPrefixAndMarkersBlobFramer();
        byte[] inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        byte[] framedBytes = (await blobFramer.FrameBlob(inputBytes)).Skip(1).ToArray();

        byte[] outputBytes = new byte[0];

        await Assert.ThrowsAsync<InvalidDataException>(async () => outputBytes = await blobFramer.UnframeBlob(framedBytes));
        Assert.Empty(outputBytes);
    }

    [Fact]
    public async Task ConfirmTruncatedEndFailure()
    {
        IBlobFramer blobFramer = new LPrefixAndMarkersBlobFramer();
        byte[] inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        byte[] framedBytes = await blobFramer.FrameBlob(inputBytes);

        byte[] outputBytes = new byte[0];
        
        await Assert.ThrowsAsync<InvalidDataException>(async () => outputBytes = await blobFramer.UnframeBlob(framedBytes.Take(framedBytes.Length/2).ToArray()));
        Assert.Empty(outputBytes);
    }
}
