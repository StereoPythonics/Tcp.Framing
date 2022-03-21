using System.Text;

namespace Tcp.Framing;

public class AcknowledgedBlobStreamer : IBlobStreamer
{
    private byte[] _blobAcknowledgement = Encoding.ASCII.GetBytes("BlobAcknowledged");
    private Stream _stream;
    private IFramedBlobStreamWriter _streamWriter;
    public AcknowledgedBlobStreamer(Stream stream, IFramedBlobStreamWriter streamWriter)
    {
        _stream = stream;
        _streamWriter = streamWriter;
    }
    public void WriteBlob(ReadOnlySpan<byte> inputBlob)
    {
        _streamWriter.WriteBlobAsFrame(inputBlob, _stream);
        WaitForBlobAcknowledgement();
    }
    public byte[] ReadBlob()
    {
        byte[] returnable = _streamWriter.ReadFrameAsBlob(_stream);
        SendBlobAcknowledgement();
        return returnable;
    }

    public void SendBlobAcknowledgement()
    {
        _stream.Write(_blobAcknowledgement);
    }

    public void WaitForBlobAcknowledgement()
    {
        byte[] check = new byte[_blobAcknowledgement.Length];
        _stream.Read(check, 0, _blobAcknowledgement.Length);
        if(! check.SequenceEqual(_blobAcknowledgement))
        {
            throw new InvalidDataException(@$"Incorrect blob acknowledgement recieved! 
                                              Expected {Encoding.ASCII.GetString(_blobAcknowledgement)}, 
                                              But got {Encoding.ASCII.GetString(check)}");
        }
    }
}
