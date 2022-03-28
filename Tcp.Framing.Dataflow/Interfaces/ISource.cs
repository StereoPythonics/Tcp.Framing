using System.Threading.Tasks.Dataflow;
namespace Tcp.Framing.Dataflow;

public interface ISource<T>
{
    BroadcastBlock<T> SourceBlock { get; }
}
