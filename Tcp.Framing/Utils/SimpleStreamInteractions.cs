namespace Tcp.Framing;

public static class SimpleStreamInteractions
{
    public static byte[] Read(this Stream stream, int count)
    {
        byte[] returnable = new byte[count];
        stream.Read(returnable, 0, count);
        return returnable;
    }

    public static async Task<byte[]> ReadAsync(this Stream stream, int count)
    {
        byte[] returnable = new byte[count];
        await stream.ReadAsync(returnable, 0, count);
        return returnable;
    }
}
