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

        await ConfirmGoodBatchedTransmisison(testSource, testSink,123,100);
    }
    [Fact]
    public async Task ConfirmReverseRoundTrip()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1241);
        ISource<TestObject> testSource = new BatchedObjectSource<TestObject>(streamPair.ClientStream);
        ISink<TestObject> testSink = new BatchedObjectSink<TestObject>(streamPair.ListenerStream, batchSize: 100);

        await ConfirmGoodBatchedTransmisison(testSource, testSink,123,100);
    }

    private async Task ConfirmGoodBatchedTransmisison(ISource<TestObject> source, ISink<TestObject> sink,int count,int batch)
    {
        var input = new TestObject(){TestString = "test123"};
        BufferBlock<TestObject> recieveBuffer = new BufferBlock<TestObject>();
        source.SourceBlock.LinkTo(recieveBuffer);
        Enumerable.Range(0,count).ToList().ForEach(i => sink.SinkBlock.Post(input));

        await Task.Delay(100);

        Assert.Equal((count/batch)*batch, recieveBuffer.Count);// Only the first batch of 100 was sent
        Assert.Equal(input.TestString, recieveBuffer.Receive().TestString);
    }
}