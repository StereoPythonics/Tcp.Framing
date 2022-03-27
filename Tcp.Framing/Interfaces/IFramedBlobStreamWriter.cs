namespace Tcp.Framing;

public interface IFramedBlobStreamWriter
{
    void WriteBlobAsFrame(byte[] bytes, Stream stream);
    byte[] ReadFrameAsBlob(Stream stream);
}

public interface IAsyncFramedBlobStreamWriter
{
    Task WriteBlobAsFrame(byte[] bytes, Stream stream);
    Task<byte[]> ReadFrameAsBlob(Stream stream);
}
