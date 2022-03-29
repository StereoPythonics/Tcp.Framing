using System.Runtime.CompilerServices;

namespace Tcp.Framing;
public class ObjectStreamer<T> : IObjectStreamer<T>, IBlockingObjectStreamer<T>, IObjectEnumerator<T>
{
    IBlobSerializer<T> _serializer;
    IBlobStreamer _blobStreamer;
    public ObjectStreamer(Stream stream, IBlobSerializer<T> serializer = null, IBlobStreamer blobStreamer = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream),"Object streamer needs a non-null stream to read/write");
        }

        _serializer = serializer ?? new GZipedJsonSerializer<T>();
        _blobStreamer = blobStreamer ?? new AcknowledgedAsyncBlobStreamer(stream,new LPrefixAndMarkersBlobFramer());
    }
    public async Task<T> ReadObjectAsync(CancellationToken cancellationToken = default) => _serializer.Deserialize(await _blobStreamer.ReadBlob(cancellationToken));
    public async Task WriteObjectAsync(T input, CancellationToken cancellationToken = default) => await _blobStreamer.WriteBlob(_serializer.Serialize(input),cancellationToken);
    public T ReadObject() => ReadObjectAsync().Result;
    public void WriteObject(T input) => WriteObjectAsync(input).RunSynchronously();

    public async IAsyncEnumerable<T> ReadAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            yield return _serializer.Deserialize(await _blobStreamer.ReadBlob(cancellationToken));
        }
    }
    public async Task WriteEnumerable(IEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        foreach(T item in source)
        {
            await _blobStreamer.WriteBlob(_serializer.Serialize(item), cancellationToken); 
        }
    }
    public async Task WriteAsyncEnumerable(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        await foreach(T item in source)
        {
            await _blobStreamer.WriteBlob(_serializer.Serialize(item), cancellationToken); 
        }
    }
    
    
    
}
