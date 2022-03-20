namespace Tcp.Framing;
public class ObjectStreamer<T> : IObjectStreamer<T>
{
    IBlobSerializer<T> _serializer;
    IBlobStreamer _blobStreamer;
    public ObjectStreamer(Stream stream, IBlobSerializer<T> serializer = null, IBlobStreamer blobStreamer = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream),"Object streamer needs a non-null stream to read/write");
        }

        _serializer = serializer ?? new DefaultJsonSerializer<T>();
        _blobStreamer = blobStreamer ?? new AcknowledgedBlobStreamer(stream,new SimpleBlobFramer());
    }
    public T ReadObject() => _serializer.Deserialize(_blobStreamer.ReadBlob());
    public void WriteObject(T input) => _blobStreamer.WriteBlob(_serializer.Serialize(input));
}
