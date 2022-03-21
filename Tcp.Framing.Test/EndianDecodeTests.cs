using Xunit;
using static Tcp.Framing.LPrefixAndMarkersBlobFramer;
namespace Tcp.Framing.Test;

public class EndianDecodeTests
{
    [Fact]
    public void ConfirmWorksOnLittleEndianMachine()
    {
        int i = 69420;
        Assert.Equal(i,EndianAwareByteDecodeInt(EndianAwareByteEncodeInt(i)));// I need a way of testing this on a bigEndian machine.
    }
}
