using System.Text;

namespace Tcp.Framing;
public class SimpleBlobFramer : IBlobFramer, IFramedBlobStream
{
    public const int bytesPerInt = 4;
    public static byte[] FrameStartMarker {get;} = Encoding.ASCII.GetBytes("StartFrame");
    public static byte[] FrameEndMarker {get;} = Encoding.ASCII.GetBytes("EndFrame");

    public byte[] FrameBlob(ReadOnlySpan<byte> input)
    {
        using MemoryStream ms = new MemoryStream();
        FrameBlob(input,ms);
        return ms.ToArray();
    }
    public byte[] UnframeBlob(ReadOnlySpan<byte> input)
    {
        using MemoryStream ms = new MemoryStream(input.ToArray());
        return UnframeBlob(ms);
    }

    public void WriteBlobAsFrame(ReadOnlySpan<byte> bytes, Stream stream)
    {
        FrameBlob(bytes,stream);
    }
    public byte[] ReadFrameAsBlob(Stream stream)
    {
        return UnframeBlob(stream);
    }
    
    public static void FrameBlob(ReadOnlySpan<byte> input, Stream stream)
    {
        stream.Write(FrameStartMarker);
        stream.Write(EndianAwareByteEncodeInt(input.Length));
        stream.Write(input);
        stream.Write(FrameEndMarker);
    }
    public static byte[] UnframeBlob(Stream stream)
    {
        ConfirmFramedMessageStart(stream);
        int frameBodyLength = GetExpectedFrameBodyLength(stream);
        byte[] returnable = stream.Read(frameBodyLength);
        ConfirmFramedMessageEnd(stream);
        return returnable;
    }
    
    public static void ConfirmFramedMessageStart(Stream stream)
    {
        var check = stream.Read(FrameStartMarker.Length);
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
    public static int GetExpectedFrameBodyLength(Stream stream)
    {
        return EndianAwareByteDecodeInt(stream.Read(bytesPerInt));
    }
    public static void ConfirmFramedMessageEnd(Stream stream)
    {
        var check = stream.Read(FrameEndMarker.Length);
        if(!check.SequenceEqual(FrameEndMarker))
        {
            throw new InvalidDataException(
                @$"Byte frame did not contain the expected message end.
                Expected {Encoding.ASCII.GetString(FrameEndMarker)}, 
                but got {Encoding.ASCII.GetString(check)}");
        }
    }

    

}
