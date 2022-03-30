using System.IO;
using System.Threading.Tasks.Dataflow;

namespace Tcp.Framing.Dataflow;

public class ObjectSink<T> : ISink<T>
{
    public BufferBlock<T> SinkBlock { get; }
    private ActionBlock<T> _streamWriterBlock;
    private ObjectStreamer<T> _objectStreamer;
    public ObjectSink(Stream stream, ObjectStreamer<T> injectedStreamer = null)
    {
        SinkBlock = new BufferBlock<T>();
        _objectStreamer = injectedStreamer ?? new ObjectStreamer<T>(stream);
        _streamWriterBlock = new ActionBlock<T>(async message => await _objectStreamer.WriteObjectAsync(message));
        SinkBlock.LinkTo(_streamWriterBlock);
    }
}
