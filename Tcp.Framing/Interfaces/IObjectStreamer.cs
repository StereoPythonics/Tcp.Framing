namespace Tcp.Framing;

public interface IObjectStreamer<T>
{
    Task<T> ReadObject();
    Task WriteObject(T input);
}

public interface IAsyncObjectStreamer<T>
{
    Task<T> ReadObject();
    Task WriteObject(T input);
}