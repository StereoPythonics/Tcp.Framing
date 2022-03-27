using Xunit;
using System.Threading.Tasks.Dataflow;
using Tcp.Framing.Test;
namespace Tcp.Framing.Dataflow.Test;

public class BatchedSourceSinkTests
{
    public class TestObject{
        public string TestString {get;set;}
    }
    [Fact]
    public async Task ConfirmRoundTrip()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1240);
        
        ISource<TestObject> testSource = new BatchedObjectSource<TestObject>(streamPair.ListenerStream);
        ISink<TestObject> testSink = new BatchedObjectSink<TestObject>(streamPair.ClientStream, batchSize: 100);

        var input = new TestObject(){TestString = "test123"};
        BufferBlock<TestObject> recieveBuffer = new BufferBlock<TestObject>();
        testSource.SourceBlock.LinkTo(recieveBuffer);
        Enumerable.Range(0,123).ToList().ForEach(i => testSink.SinkBlock.Post(input));

        await Task.Delay(100);

        Assert.Equal(100, recieveBuffer.Count);
        Assert.Equal(input.TestString, recieveBuffer.Receive().TestString);
        
    }
    [Fact]
    public async Task ConfirmReverseRoundTrip()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1241);
        
        ISource<TestObject> testSource = new BatchedObjectSource<TestObject>(streamPair.ClientStream);
        ISink<TestObject> testSink = new BatchedObjectSink<TestObject>(streamPair.ListenerStream, batchSize: 100);

        var input = new TestObject(){TestString = "test123"};
        BufferBlock<TestObject> recieveBuffer = new BufferBlock<TestObject>();
        testSource.SourceBlock.LinkTo(recieveBuffer);
        Enumerable.Range(0,123).ToList().ForEach(i => testSink.SinkBlock.Post(input));

        await Task.Delay(100);

        Assert.Equal(100, recieveBuffer.Count);
        Assert.Equal(input.TestString, recieveBuffer.Receive().TestString);
    }
}