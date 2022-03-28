using System.IO.Compression;
namespace Tcp.Framing;

using System.Text.Json;

public class GZipedJsonSerializer<T> : IBlobSerializer<T>
{
    public T Deserialize(byte[] inputBytes)
    {
        using MemoryStream decompressedStream = new MemoryStream();
        using (MemoryStream compressedStream = new MemoryStream(inputBytes))
        {
            using GZipStream decompressor = new GZipStream(compressedStream, CompressionMode.Decompress);
            decompressor.CopyTo(decompressedStream);
        }
        decompressedStream.Seek(0,SeekOrigin.Begin);
        return JsonSerializer.Deserialize<T>(decompressedStream);

    } 
    public byte[] Serialize(T inputObject)
    {
        using MemoryStream msout = new MemoryStream();
        using (MemoryStream msin = new MemoryStream())
        {
            JsonSerializer.Serialize(msin,inputObject);
            msin.Seek(0,SeekOrigin.Begin);
        
            using GZipStream compressor = new GZipStream(msout, CompressionMode.Compress);
            msin.CopyTo(compressor);
        }
        return msout.ToArray();
    }
}
