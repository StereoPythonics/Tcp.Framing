namespace Tcp.Framing;

public interface IFramedBlobStream
{
    void WriteBlobAsFrame(ReadOnlySpan<byte> bytes, Stream stream);
    byte[] ReadFrameAsBlob(Stream stream);
}
public interface IBlobFramer
{
    byte[] FrameBlob(ReadOnlySpan<byte> bytes);
    byte[] UnframeBlob(ReadOnlySpan<byte> bytes);
}
