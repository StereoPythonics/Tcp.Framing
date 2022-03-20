namespace Tcp.Framing;

using System.Text.Json;
public class DefaultJsonSerializer<T> : IBlobSerializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> inputBytes) => JsonSerializer.Deserialize<T>(inputBytes);
    public byte[] Serialize(T inputObject)
    {
        using MemoryStream ms = new MemoryStream();
        JsonSerializer.Serialize(ms,inputObject);
        return ms.ToArray();
    }
}
