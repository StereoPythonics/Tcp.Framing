using Xunit;
namespace Tcp.Framing.Test;

public class EndianDecodeTests
{
    [Fact]
    public void ConfirmWorksOnLittleEndianMachine()
    {
        int i = 69420;
        Assert.Equal(i,LPrefixAndMarkersBlobFramer.EndianAwareByteDecodeInt(LPrefixAndMarkersBlobFramer.EndianAwareByteEncodeInt(i)));// I need a way of testing this on a bigEndian machine.
    }
}
