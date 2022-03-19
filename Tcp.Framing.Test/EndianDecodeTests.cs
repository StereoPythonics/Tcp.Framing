using Xunit;
using static Tcp.Framing.SimpleBlobFramer;
namespace Tcp.Framing.Test;

public class EndianDecodeTests
{
    [Fact]
    public void ConfirmWorksOnLittleEndianMachine()
    {
        int i = 69420;
        Assert.Equal(i,EndianAwareByteDecodeInt(EndianAwareByteEncodeInt(i)));
    }
}
