namespace Tcp.Framing;

public interface IObjectStreamer<T>
{
    Task<T> ReadObjectAsync();
    Task WriteObjectAsync(T input);
}
