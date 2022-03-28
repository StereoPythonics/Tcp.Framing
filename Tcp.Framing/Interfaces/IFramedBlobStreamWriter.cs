namespace Tcp.Framing;

public interface IFramedBlobStreamWriter
{
    Task WriteBlobAsFrame(byte[] bytes, Stream stream);
    Task<byte[]> ReadFrameAsBlob(Stream stream);
}
