namespace Tcp.Framing;

public static class SimpleStreamInteractions
{
    public static byte[] Read(this Stream stream, int count)
    {
        byte[] returnable = new byte[count];
        stream.Read(returnable, 0, count);
        return returnable;
    }
}
