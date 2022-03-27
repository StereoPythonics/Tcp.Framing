using System.Text;

namespace Tcp.Framing;
public class LPrefixAndMarkersBlobFramer : IAsyncBlobFramer, IAsyncFramedBlobStreamWriter
{
    public const int bytesPerInt = 4;
    public static byte[] FrameStartMarker {get;} = Encoding.ASCII.GetBytes("StartFrame");
    public static byte[] FrameEndMarker {get;} = Encoding.ASCII.GetBytes("EndFrame");

    public async Task<byte[]> FrameBlob(byte[] input)
    {
        using MemoryStream ms = new MemoryStream();
        await FrameBlobAsync(input,ms);
        return ms.ToArray();
    }
    public async Task<byte[]> UnframeBlob(byte[] input)
    {
        using MemoryStream ms = new MemoryStream(input.ToArray());
        return await UnframeBlobAsync(ms);
    }

    public async Task WriteBlobAsFrame(byte[] bytes, Stream stream)
    {
        await FrameBlobAsync(bytes,stream);
    }
    public async Task<byte[]> ReadFrameAsBlob(Stream stream)
    {
        return await UnframeBlobAsync(stream);
    }
    
    public static async Task FrameBlobAsync(byte[] input, Stream stream)
    {
        await stream.WriteAsync(FrameStartMarker);
        await stream.WriteAsync(EndianAwareByteEncodeInt(input.Length));
        await stream.WriteAsync(input);
        await stream.WriteAsync(FrameEndMarker);
    }
    public static async Task<byte[]> UnframeBlobAsync(Stream stream)
    {
        await ConfirmFramedMessageStartAsync(stream);
        int frameBodyLength = await GetExpectedFrameBodyLengthAsync(stream);
        byte[] returnable = await stream.ReadAsync(frameBodyLength);
        await ConfirmFramedMessageEnd(stream);
        return returnable;
    }
    
    public static async Task ConfirmFramedMessageStartAsync(Stream stream)
    {
        var check = await stream.ReadAsync(FrameStartMarker.Length);
        if(!check.SequenceEqual(FrameStartMarker))
        {
            throw new InvalidDataException(
                @$"Byte frame did not contain the expected message start.
                Expected {Encoding.ASCII.GetString(FrameStartMarker)}, 
                but got {Encoding.ASCII.GetString(check)}");
        }
    }
    public static byte[] EndianAwareByteEncodeInt(int i) {
        return BitConverter.IsLittleEndian ?
            BitConverter.GetBytes(i) :
            BitConverter.GetBytes(i).Reverse().ToArray();
    }
    public static int EndianAwareByteDecodeInt(byte[] bytes) {
        return BitConverter.IsLittleEndian ? 
            BitConverter.ToInt32(bytes) : 
            BitConverter.ToInt32(bytes.Reverse().ToArray());
    }
    public static async Task<int> GetExpectedFrameBodyLengthAsync(Stream stream)
    {
        return EndianAwareByteDecodeInt(await stream.ReadAsync(bytesPerInt));
    }
    public static async Task ConfirmFramedMessageEnd(Stream stream)
    {
        var check = await  stream.ReadAsync(FrameEndMarker.Length);
        if(!check.SequenceEqual(FrameEndMarker))
        {
            throw new InvalidDataException(
                @$"Byte frame did not contain the expected message end.
                Expected {Encoding.ASCII.GetString(FrameEndMarker)}, 
                but got {Encoding.ASCII.GetString(check)}");
        }
    }
}
