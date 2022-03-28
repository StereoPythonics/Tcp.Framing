namespace Tcp.Framing;

public interface IBlobFramer
{
    Task<byte[]> FrameBlob(byte[] bytes);
    Task<byte[]> UnframeBlob(byte[] bytes);
}
