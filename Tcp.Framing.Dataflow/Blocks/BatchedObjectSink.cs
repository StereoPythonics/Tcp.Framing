using System.IO;
using System.Threading.Tasks.Dataflow;

namespace Tcp.Framing.Dataflow;

public class BatchedObjectSink<T> : ISink<T>
{
    public BufferBlock<T> SinkBlock {get;}
    private BatchBlock<T> batchBlock;
    private ActionBlock<T[]> _streamWriterBlock;
    private ObjectStreamer<T[]> _objectStreamer;
    public BatchedObjectSink(Stream stream, IObjectStreamer<T[]> objectStreamer = null, int batchSize = 100)
    {
        SinkBlock = new BufferBlock<T>();
        batchBlock = new BatchBlock<T>(batchSize);
        _objectStreamer ??= new ObjectStreamer<T[]>(stream);
        _streamWriterBlock = new ActionBlock<T[]>(async messageArray => await _objectStreamer.WriteObjectAsync(messageArray));
        SinkBlock.LinkTo(batchBlock);
        batchBlock.LinkTo(_streamWriterBlock);
    }
}
