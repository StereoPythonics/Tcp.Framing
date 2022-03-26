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

        var input = new TestObject(){TestString = "test123"};
        testSink.SinkBlock.Post(input);
        var output = testSource.SourceBlock.Receive();
        Assert.Equal(input.TestString, output.TestString);
    }
}