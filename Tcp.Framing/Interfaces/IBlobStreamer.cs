namespace Tcp.Framing;
public interface IBlobStreamer
{
    Task<byte[]> ReadBlob(CancellationToken cancellationToken = default);
    Task WriteBlob(byte[] input,CancellationToken cancellationToken = default);
}