using System.Text;

namespace Tcp.Framing;

public class AcknowledgedAsyncBlobStreamer : IBlobStreamer
{
    private byte[] _blobAcknowledgement = Encoding.ASCII.GetBytes("BlobAcknowledged");
    private Stream _stream;
    private IFramedBlobStreamWriter _streamWriter;
    public event EventHandler<EventArgs> ConnectionDropped;

    public AcknowledgedAsyncBlobStreamer(Stream stream, IFramedBlobStreamWriter streamWriter)
    {
        _stream = stream;
        _streamWriter = streamWriter;
        streamWriter.ConnectionDropped += (o,e) => ConnectionDropped.Invoke(o,e);
    }

    public async Task WriteBlob(byte[] inputBlob, CancellationToken cancellationToken = default)
    {
        await _streamWriter.WriteBlobAsFrame(inputBlob, _stream, cancellationToken);
        await WaitForBlobAcknowledgement(cancellationToken);
    }
    public async Task<byte[]> ReadBlob(CancellationToken cancellationToken = default)
    {
        byte[] returnable = await _streamWriter.ReadFrameAsBlob(_stream, cancellationToken);
        await SendBlobAcknowledgement(cancellationToken);
        return returnable;
    }
    public async Task SendBlobAcknowledgement(CancellationToken cancellationToken = default)
    {
        await _stream.WriteAsync(_blobAcknowledgement, cancellationToken);
    }

    public async Task WaitForBlobAcknowledgement(CancellationToken cancellationToken = default)
    {
        byte[] check = await _stream.ReadAsync(_blobAcknowledgement.Length, cancellationToken);
        if(!check.Any())
        {
            ConnectionDropped.Invoke(null,null);
        }
        if(! check.SequenceEqual(_blobAcknowledgement))
        {
            throw new InvalidDataException(@$"Incorrect blob acknowledgement recieved! 
                                              Expected {Encoding.ASCII.GetString(_blobAcknowledgement)}, 
                                              But got {Encoding.ASCII.GetString(check)}");
        }
    }
}
