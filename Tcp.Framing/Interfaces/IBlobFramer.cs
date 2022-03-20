namespace Tcp.Framing;


public interface IBlobFramer
{
    byte[] FrameBlob(ReadOnlySpan<byte> bytes);
    byte[] UnframeBlob(ReadOnlySpan<byte> bytes);
}
