namespace Tcp.Framing;
public interface IBlobStreamer
{
    Task<byte[]> ReadBlob();
    Task WriteBlob(byte[] input);
}