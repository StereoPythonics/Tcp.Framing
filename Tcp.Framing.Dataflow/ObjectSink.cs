using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tcp.Framing.Dataflow;

public class ObjectSink<T> : ISink<T>
{
    public BufferBlock<T> SinkBlock { get; }
    private ActionBlock<T> _streamWriterBlock;
    private ObjectStreamer<T> _objectStreamer;
    public ObjectSink(Stream stream)
    {
        SinkBlock = new BufferBlock<T>();
        _objectStreamer = new ObjectStreamer<T>(stream);
        _streamWriterBlock = new ActionBlock<T>(message => _objectStreamer.WriteObject(message));
    }
}
public class ObjectSource<T> : ISource<T>
{
    public BroadcastBlock<T> SourceBlock { get; }
    private ObjectStreamer<T> _objectStreamer;
    public ObjectSource(Stream stream)
    {
        SourceBlock = new BroadcastBlock<T>(m => m);
        _objectStreamer = new ObjectStreamer<T>(stream);

        Task.Run(() =>
        {
            while(true)
            {
                SourceBlock.Post(_objectStreamer.ReadObject());
            }
        });
    }



}