namespace Tcp.Framing;

public interface IAsyncObjectStreamer<T>
{
    Task<T> ReadObjectAsync();
    Task WriteObjectAsync(T input);
}
