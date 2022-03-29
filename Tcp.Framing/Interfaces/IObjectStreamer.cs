namespace Tcp.Framing;

public interface IObjectStreamer<T>
{
    Task<T> ReadObjectAsync(CancellationToken cancellationToken = default);
    Task WriteObjectAsync(T input, CancellationToken cancellationToken = default);
}

public interface IObjectEnumerator<T>
{
    IAsyncEnumerable<T> ReadAsyncEnumerable(CancellationToken cancellationToken = default);
    Task WriteAsyncEnumerable(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default);
    Task WriteEnumerable(IEnumerable<T> source, CancellationToken cancellationToken = default);
}
