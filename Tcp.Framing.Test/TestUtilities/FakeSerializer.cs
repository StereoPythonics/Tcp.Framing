using Tcp.Framing;

public class FakeSerializer<T> : IBlobSerializer<T> //For benchmarking use only
{
    T _standIn;
    byte[] _byteStandin;
    public FakeSerializer(T standin, int byteSize)
    {
        _standIn = standin;
        _byteStandin = new byte[byteSize];
    }

    public T Deserialize(byte[] inputBytes)
    {
        return _standIn;
    }

    public byte[] Serialize(T inputObject)
    {
        return _byteStandin;
    }
}