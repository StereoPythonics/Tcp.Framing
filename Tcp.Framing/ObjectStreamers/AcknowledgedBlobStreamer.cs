using System.Text;

namespace Tcp.Framing;

public class AcknowledgedBlobStreamer : IAsyncBlobStreamer
{
    private byte[] _blobAcknowledgement = Encoding.ASCII.GetBytes("BlobAcknowledged");
    private Stream _stream;
    private IAsyncFramedBlobStreamWriter _streamWriter;
    public AcknowledgedBlobStreamer(Stream stream, IAsyncFramedBlobStreamWriter streamWriter)
    {
        _stream = stream;
        _streamWriter = streamWriter;
    }

     public async Task WriteBlob(byte[] inputBlob)
    {
        await _streamWriter.WriteBlobAsFrame(inputBlob, _stream);
        await WaitForBlobAcknowledgement();
    }
    public async Task<byte[]> ReadBlob()
    {
        byte[] returnable = await _streamWriter.ReadFrameAsBlob(_stream);
        await SendBlobAcknowledgement();
        return returnable;
    }
    public async Task SendBlobAcknowledgement()
    {
        await _stream.WriteAsync(_blobAcknowledgement);
    }

    public async 
    Task WaitForBlobAcknowledgement()
    {
        byte[] check = new byte[_blobAcknowledgement.Length];
        await _stream.ReadAsync(check, 0, _blobAcknowledgement.Length);
        if(! check.SequenceEqual(_blobAcknowledgement))
        {
            throw new InvalidDataException(@$"Incorrect blob acknowledgement recieved! 
                                              Expected {Encoding.ASCII.GetString(_blobAcknowledgement)}, 
                                              But got {Encoding.ASCII.GetString(check)}");
        }
    }
}
