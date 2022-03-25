using Xunit;
using System.Threading.Tasks.Dataflow;

namespace Tcp.Framing.Dataflow.Test;

public class SourceSinkTests
{
    public class TestObject{
        public string TestString {get;set;}
    }
    [Fact]
    public void ConfirmRoundTrip()
    {
        using MemoryStream ms = new MemoryStream();
        ISource<TestObject> testSource = new ObjectSource<TestObject>(ms);
        ISink<TestObject> testSink = new ObjectSink<TestObject>(ms);

        var input = new TestObject(){TestString = "test123"};
        testSink.SinkBlock.Post(input);
        var output = testSource.SourceBlock.Receive();
        Assert.Equal(input.TestString, output.TestString);
    }
}