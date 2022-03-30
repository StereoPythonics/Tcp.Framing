namespace Tcp.Framing;

using System.Text.Json;
public class UTF8JsonSerializer<T> : IBlobSerializer<T>
{
    public T Deserialize(byte[] inputBytes) => JsonSerializer.Deserialize<T>(inputBytes);
    public byte[] Serialize(T inputObject)
    {
        using MemoryStream ms = new MemoryStream();
        JsonSerializer.Serialize(ms,inputObject);
        return ms.ToArray();
    }
}
