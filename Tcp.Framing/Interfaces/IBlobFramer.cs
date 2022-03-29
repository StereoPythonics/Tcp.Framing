namespace Tcp.Framing;

public interface IBlobFramer
{
    Task<byte[]> FrameBlob(byte[] bytes, CancellationToken cancellationToken = default);
    Task<byte[]> UnframeBlob(byte[] bytes, CancellationToken cancellationToken = default);
}
