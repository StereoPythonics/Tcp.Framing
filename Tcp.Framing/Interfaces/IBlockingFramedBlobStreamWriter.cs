namespace Tcp.Framing;

public interface IBlockingFramedBlobStreamWriter
{
    void WriteBlobAsFrame(byte[] bytes, Stream stream);
    byte[] ReadFrameAsBlob(Stream stream);
}
