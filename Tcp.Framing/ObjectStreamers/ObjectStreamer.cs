namespace Tcp.Framing;
public class ObjectStreamer<T> : IAsyncObjectStreamer<T>, IBlockingObjectStreamer<T>
{
    IBlobSerializer<T> _serializer;
    IAsyncBlobStreamer _blobStreamer;
    public ObjectStreamer(Stream stream, IBlobSerializer<T> serializer = null, IAsyncBlobStreamer blobStreamer = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream),"Object streamer needs a non-null stream to read/write");
        }

        _serializer = serializer ?? new DefaultJsonSerializer<T>();
        _blobStreamer = blobStreamer ?? new AcknowledgedBlobStreamer(stream,new LPrefixAndMarkersBlobFramer());
    }
    public async Task<T> ReadObjectAsync() => _serializer.Deserialize(await _blobStreamer.ReadBlob());
    public async Task WriteObjectAsync(T input) => await _blobStreamer.WriteBlob(_serializer.Serialize(input));
    public T ReadObject() => ReadObjectAsync().Result;
    public void WriteObject(T input) => WriteObjectAsync(input).RunSynchronously();
}
