namespace Tcp.Framing;

public interface IObjectEnumerator<T>
{
    IAsyncEnumerable<T> ReadAsyncEnumerable(CancellationToken cancellationToken = default);
    Task WriteAsyncEnumerable(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default);
    Task WriteEnumerable(IEnumerable<T> source, CancellationToken cancellationToken = default);
}
