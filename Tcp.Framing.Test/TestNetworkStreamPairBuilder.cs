using System.Net.Sockets;
using System.Net;

namespace Tcp.Framing.Test;

public class TestNetworkStreamPairBuilder
{   
    public static TestStreamPair GetTestStreamPair(int port)
    {
        TestStreamPair returnable = new TestStreamPair();
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),port);
        listener.Start();

        var getListenerStream = Task.Run(() => {
            returnable.ListenerClient = listener.AcceptTcpClient();
            returnable.ListenerStream = returnable.ListenerClient.GetStream();
        });

        TcpClient readClient = new TcpClient();
        readClient.Connect("127.0.0.1",port);
        returnable.ClientStream = readClient.GetStream();
        returnable.ClientClient = readClient;
        getListenerStream.Wait();
        
        return returnable;

    }  
    public class TestStreamPair
    {
        public NetworkStream ListenerStream;
        public TcpClient ListenerClient;
        public NetworkStream ClientStream;
        public TcpClient ClientClient;
    }

}
