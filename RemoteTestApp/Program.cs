using System.Net.Sockets;
using System.Net;
using System;
using System.Threading.Tasks;
using Tcp.Framing;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

class Program
{

    public static void Main()
    {
        Console.WriteLine("Bouncer backer");

        //Setup serverside listener
        TcpListener receiveListener = new TcpListener(IPAddress.Parse("0.0.0.0"), 4456);
        receiveListener.Start();

        TcpListener sendListener = new TcpListener(IPAddress.Parse("0.0.0.0"), 4457);
        sendListener.Start();

        IObjectEnumerator<ExampleTestObject> receiveObjectStreamer = null;
        IObjectEnumerator<ExampleTestObject> sendObjectStreamer = null;
        IBlobSerializer<ExampleTestObject> serializer = new GZipedJsonSerializer<ExampleTestObject>();

        Console.WriteLine("Listeners set up, waiting for conections");
        //Handle incomming connection on background thread
        TcpClient receiveClient = null;
        NetworkStream receiveserverNetworkStream = null;
        var receiverSetUp = Task.Run(() =>
        {
            receiveClient = receiveListener.AcceptTcpClient(); //blocks waiting for client connection
            receiveserverNetworkStream = receiveClient.GetStream();
            receiveObjectStreamer = new ObjectStreamer<ExampleTestObject>(receiveserverNetworkStream, serializer);
            Console.WriteLine("receive connection established");
        });

        TcpClient senderClient = null;
        NetworkStream senderServerNetworkStream = null;
        var senderSetUp = Task.Run(() =>
        {
            senderClient = sendListener.AcceptTcpClient(); //blocks waiting for client connection
            senderServerNetworkStream = senderClient.GetStream();
            sendObjectStreamer = new ObjectStreamer<ExampleTestObject>(senderServerNetworkStream, serializer);
            Console.WriteLine("send connection established");
        });

        Task.WaitAll(receiverSetUp, senderSetUp);
        var source = new CancellationTokenSource();
        int i = 0;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        var pipeBack = Task.Run(async () =>
        {
            try{
            Console.WriteLine("Initiating Pipeback");
            await sendObjectStreamer.WriteAsyncEnumerable(
                receiveObjectStreamer.ReadAsyncEnumerable(source.Token).Select(m => {if(i++%100 == 0)Console.WriteLine($"Message{i}"); return m;})
                , source.Token);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            sw.Stop();
            var msPerMessage = sw.ElapsedMilliseconds*1.0 / i;
            Console.WriteLine($"Performance = {msPerMessage}ms per message");
        });

        Console.WriteLine("Press enter key to terminate");
        Console.ReadLine();
        source.Cancel();
    }
}

public record ExampleTestObject{
        public string ExampleString {get;set;}
        public double ExampleDouble {get;set;}
        public int ExampleInt {get;set;}
    }