namespace Tcp.Framing;

public interface IBlobSerializer<T>
{
    byte[] Serialize(T inputObject);
    T Deserialize(ReadOnlySpan<byte> inputBytes);
}
