using System.Text;

namespace Tcp.Framing;
public class LPrefixAndMarkersBlobFramer : IBlobFramer, IFramedBlobStreamWriter
{
    public const int bytesPerInt = 4;

    public event EventHandler<EventArgs> ConnectionDropped;

    public static byte[] FrameStartMarker {get;} = Encoding.ASCII.GetBytes("StartFrame");
    public static byte[] FrameEndMarker {get;} = Encoding.ASCII.GetBytes("EndFrame");

    public async Task<byte[]> FrameBlob(byte[] input, CancellationToken cancellationToken = default)
    {
        using MemoryStream ms = new MemoryStream();
        await FrameBlobAsync(input,ms, cancellationToken);
        return ms.ToArray();
    }
    public async Task<byte[]> UnframeBlob(byte[] input, CancellationToken cancellationToken = default)
    {
        using MemoryStream ms = new MemoryStream(input.ToArray());
        return await UnframeBlobAsync(ms, cancellationToken);
    }

    public async Task WriteBlobAsFrame(byte[] bytes, Stream stream, CancellationToken cancellationToken = default)
    {
        await FrameBlobAsync(bytes,stream, cancellationToken);
    }
    public async Task<byte[]> ReadFrameAsBlob(Stream stream, CancellationToken cancellationToken = default)
    {
        return await UnframeBlobAsync(stream, cancellationToken);
    }
    
    public async Task FrameBlobAsync(byte[] input, Stream stream, CancellationToken cancellationToken = default)
    {
        using MemoryStream ms = new MemoryStream();
        ms.Write(FrameStartMarker);
        ms.Write(EndianAwareByteEncodeInt(input.Length));
        ms.Write(input);
        ms.Write(FrameEndMarker);
        ms.Seek(0,SeekOrigin.Begin);
        await ms.CopyToAsync(stream, cancellationToken);
    }
    public async Task<byte[]> UnframeBlobAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await ConfirmFramedMessageStartAsync(stream, cancellationToken);
        int frameBodyLength = await GetExpectedFrameBodyLengthAsync(stream, cancellationToken);
        byte[] returnable = await stream.ReadAsync(frameBodyLength, cancellationToken);
        await ConfirmFramedMessageEnd(stream, cancellationToken);
        return returnable;
    }
    
    public async Task ConfirmFramedMessageStartAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var check = await stream.ReadAsync(FrameStartMarker.Length, cancellationToken);
        if(!check.Any())
        {
            ConnectionDropped.Invoke(null,null);
        }
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
    public async Task<int> GetExpectedFrameBodyLengthAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return EndianAwareByteDecodeInt(await stream.ReadAsync(bytesPerInt, cancellationToken));
    }
    public async Task ConfirmFramedMessageEnd(Stream stream, CancellationToken cancellationToken = default)
    {
        var check = await stream.ReadAsync(FrameEndMarker.Length, cancellationToken);
        if(!check.Any())
        {
            ConnectionDropped.Invoke(null,null);
        }
        if(!check.SequenceEqual(FrameEndMarker))
        {
            ConnectionDropped.Invoke(null,null);
            throw new InvalidDataException(
                @$"Byte frame did not contain the expected message end.
                Expected {Encoding.ASCII.GetString(FrameEndMarker)}, 
                but got {Encoding.ASCII.GetString(check)}");
        }
    }
}
