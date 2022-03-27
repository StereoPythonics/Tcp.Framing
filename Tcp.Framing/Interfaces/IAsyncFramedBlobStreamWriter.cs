namespace Tcp.Framing;

public interface IAsyncFramedBlobStreamWriter
{
    Task WriteBlobAsFrame(byte[] bytes, Stream stream);
    Task<byte[]> ReadFrameAsBlob(Stream stream);
}
