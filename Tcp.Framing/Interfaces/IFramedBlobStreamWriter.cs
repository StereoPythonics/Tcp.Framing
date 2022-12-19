namespace Tcp.Framing;

public interface IFramedBlobStreamWriter
{
    Task WriteBlobAsFrame(byte[] bytes, Stream stream, CancellationToken cancellationToken = default);
    Task<byte[]> ReadFrameAsBlob(Stream stream, CancellationToken cancellationToken = default);
    event EventHandler<EventArgs> ConnectionDropped;
}
