namespace Tcp.Framing;

public interface IBlobStreamer
{
    byte[] ReadBlob();
    void WriteBlob(byte[] input);
}
public interface IAsyncBlobStreamer
{
    Task<byte[]> ReadBlob();
    Task WriteBlob(byte[] input);
}