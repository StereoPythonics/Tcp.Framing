namespace Tcp.Framing;

public interface IObjectStreamer<T>
{
    Task<T> ReadObjectAsync(CancellationToken cancellationToken = default);
    Task WriteObjectAsync(T input, CancellationToken cancellationToken = default);
}
