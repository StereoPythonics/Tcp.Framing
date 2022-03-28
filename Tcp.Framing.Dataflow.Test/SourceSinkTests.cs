using Xunit;
using System.Threading.Tasks.Dataflow;
using Tcp.Framing.Test;
namespace Tcp.Framing.Dataflow.Test;

public class SourceSinkTests
{
    public class TestObject{
        public string TestString {get;set;}
    }
    [Fact]
    public void ConfirmRoundTrip()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1237);
        ISource<TestObject> testSource = new ObjectSource<TestObject>(streamPair.ListenerStream);
        ISink<TestObject> testSink = new ObjectSink<TestObject>(streamPair.ClientStream);

        ConfirmGoodTransmisison(testSource,testSink);
    }
    [Fact]
    public void ConfirmReverseRoundTrip()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1238);
        ISource<TestObject> testSource = new ObjectSource<TestObject>(streamPair.ClientStream);
        ISink<TestObject> testSink = new ObjectSink<TestObject>(streamPair.ListenerStream);

        ConfirmGoodTransmisison(testSource,testSink);
    }

    private void ConfirmGoodTransmisison(ISource<TestObject> source, ISink<TestObject> sink)
    {
        var input = new TestObject(){TestString = "test123"};
        sink.SinkBlock.Post(input);
        var output = source.SourceBlock.Receive();
        Assert.Equal(input.TestString, output.TestString);
    }
}