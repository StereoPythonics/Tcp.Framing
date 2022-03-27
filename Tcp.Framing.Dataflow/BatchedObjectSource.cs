using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tcp.Framing.Dataflow;

public class BatchedObjectSource<T> : ISource<T>
{
    public BroadcastBlock<T> SourceBlock { get; }
    private IAsyncObjectStreamer<T[]> _objectStreamer;
    public BatchedObjectSource(Stream stream, IAsyncObjectStreamer<T[]> objectStreamer = null)
    {
        SourceBlock = new BroadcastBlock<T>(m => m);
        _objectStreamer ??= new ObjectStreamer<T[]>(stream);

        Task.Run(async () =>
        {
            while(true)
            {   
                var batch = await _objectStreamer.ReadObjectAsync();
                foreach (var message in batch)
                {
                    SourceBlock.Post(message);
                }
            }
        });
    }
}