using System.Threading.Tasks.Dataflow;
namespace Tcp.Framing.Dataflow;

public interface ISink<T>
{
    BufferBlock<T> SinkBlock { get; }
}
