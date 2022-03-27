namespace Tcp.Framing;

public interface IBlockingObjectStreamer<T>
{
    T ReadObject();
    void WriteObject(T input);
}