using Xunit;
using System.Text;
namespace Tcp.Framing.Test;

public class RountTripFramingTests
{
    [Fact]
    public void ConfirmRountTripStreamSuccess()
    {
        IFramedBlobStream blobStream = new SimpleBlobFramer();
        using MemoryStream ms = new MemoryStream();
        var inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        blobStream.WriteBlobAsFrame(inputBytes,ms);
        ms.Seek(0,SeekOrigin.Begin);

        var outputBytes = blobStream.ReadFrameAsBlob(ms);

        Assert.True(inputBytes.SequenceEqual(outputBytes));
    }

    [Fact]
    public void ConfirmMissingStartFailure()
    {
        IBlobFramer blobFramer = new SimpleBlobFramer();
        var inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        var intermediateBytes = blobFramer.FrameBlob(inputBytes).Skip(1).ToArray();

        byte[] outputBytes = new byte[0];

        Assert.Throws<InvalidDataException>(() => outputBytes = blobFramer.UnframeBlob(intermediateBytes));
        Assert.Empty(outputBytes);
    }

    [Fact]
    public void ConfirmTruncatedEndFailure()
    {
        IBlobFramer blobFramer = new SimpleBlobFramer();
        var inputBytes = Encoding.ASCII.GetBytes("ExampleTestString");

        var intermediateBytes = blobFramer.FrameBlob(inputBytes);
        byte[] outputBytes = new byte[0];
        
        Assert.Throws<InvalidDataException>(() => outputBytes = blobFramer.UnframeBlob(intermediateBytes.Take(intermediateBytes.Length/2).ToArray()));
        Assert.Empty(outputBytes);
    }
}