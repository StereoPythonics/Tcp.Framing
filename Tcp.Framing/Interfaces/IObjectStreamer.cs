namespace Tcp.Framing;

public interface IObjectStreamer<T>
{
    T ReadObject();
    void WriteObject(T input);
}
