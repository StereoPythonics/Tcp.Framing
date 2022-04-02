namespace Tcp.Framing;

public static class SimpleStreamInteractions
{
    public static byte[] Read(this Stream stream, int count)
    {
        byte[] returnable = new byte[count];
        stream.Read(returnable, 0, count);
        return returnable;
    }

    public static async Task<byte[]> ReadAsync(this Stream stream, int count, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[count];
        MemoryStream returnable = new MemoryStream();
        
        int remainingBytes = count;

        while(remainingBytes > 0)
        {
            int bytesBack = await stream.ReadAsync(buffer, 0, remainingBytes, cancellationToken);
            remainingBytes -= bytesBack;
            returnable.Write(buffer,0,bytesBack);
            if(bytesBack == 0) throw new InvalidDataException($@"Expected {count} readable bytes, 
             but was only able to retrieve {count-remainingBytes} before the end of the stream.");
        }
        
        return returnable.ToArray();
    }
}
