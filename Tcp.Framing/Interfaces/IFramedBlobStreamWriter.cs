namespace Tcp.Framing;

public interface IFramedBlobStreamWriter
{
    void WriteBlobAsFrame(ReadOnlySpan<byte> bytes, Stream stream);
    byte[] ReadFrameAsBlob(Stream stream);
}
