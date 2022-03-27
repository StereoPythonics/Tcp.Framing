namespace Tcp.Framing;


public interface IBlobFramer
{
    byte[] FrameBlob(byte[] bytes);
    byte[] UnframeBlob(byte[] bytes);
}

public interface IAsyncBlobFramer
{
    Task<byte[]> FrameBlob(byte[] bytes);
    Task<byte[]> UnframeBlob(byte[] bytes);
}
