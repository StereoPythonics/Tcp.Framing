namespace Tcp.Framing;

public interface IBlobStreamer
{
    byte[] ReadBlob();
    void WriteBlob(ReadOnlySpan<byte> input);
}