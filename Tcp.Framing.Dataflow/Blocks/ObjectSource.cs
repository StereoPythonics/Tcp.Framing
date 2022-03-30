using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tcp.Framing.Dataflow;

public class ObjectSource<T> : ISource<T>
{
    public BroadcastBlock<T> SourceBlock { get; }
    private ObjectStreamer<T> _objectStreamer;
    public ObjectSource(Stream stream, ObjectStreamer<T> injectedStreamer = null)
    {
        SourceBlock = new BroadcastBlock<T>(m => m);
        _objectStreamer = injectedStreamer ?? new ObjectStreamer<T>(stream);

        Task.Run(async () =>
        {
            while(true)
            {
                SourceBlock.Post(await _objectStreamer.ReadObjectAsync());
            }
        });
    }



}