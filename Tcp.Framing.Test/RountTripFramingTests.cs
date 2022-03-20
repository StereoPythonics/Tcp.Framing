using Xunit;
using System.Text;
namespace Tcp.Framing.Test;
public class RoundTripFramingTests
{
    [Fact]
    public void ConfirmRountTripStreamSuccess()
    {
        IFramedBlobStreamWriter blobStream = new SimpleBlobFramer();
        using MemoryStream ms = new MemoryStream();
        byte[] inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        blobStream.WriteBlobAsFrame(inputBytes,ms);
        ms.Seek(0,SeekOrigin.Begin);

        byte[] outputBytes = blobStream.ReadFrameAsBlob(ms);

        Assert.True(inputBytes.SequenceEqual(outputBytes));
    }

    [Fact]
    public void ConfirmMissingStartFailure()
    {
        IBlobFramer blobFramer = new SimpleBlobFramer();
        byte[] inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        byte[] framedBytes = blobFramer.FrameBlob(inputBytes).Skip(1).ToArray();

        byte[] outputBytes = new byte[0];

        Assert.Throws<InvalidDataException>(() => outputBytes = blobFramer.UnframeBlob(framedBytes));
        Assert.Empty(outputBytes);
    }

    [Fact]
    public void ConfirmTruncatedEndFailure()
    {
        IBlobFramer blobFramer = new SimpleBlobFramer();
        byte[] inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        byte[] framedBytes = blobFramer.FrameBlob(inputBytes);

        byte[] outputBytes = new byte[0];
        
        Assert.Throws<InvalidDataException>(() => outputBytes = blobFramer.UnframeBlob(framedBytes.Take(framedBytes.Length/2).ToArray()));
        Assert.Empty(outputBytes);
    }
}