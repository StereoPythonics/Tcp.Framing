using Xunit;

namespace Tcp.Framing.Test;
public class RoundTripObjectStreamingTests
{
    [Fact]
    public void TestSingleObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1235);

        Task.Run(() => {
        IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
        serverObjectStreamer.WriteObject(input);
        });

        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
        Assert.Equal(input,clientObjectStreamer.ReadObject());
    }

    [Fact]
    public void TestMultiObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1236);

        Task.Run(() => {
            IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
            Enumerable.Range(0,100).ToList().ForEach(i => serverObjectStreamer.WriteObject(input));
        });
        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
        Enumerable.Range(0,100).ToList().ForEach(i => Assert.True(input.Equals(clientObjectStreamer.ReadObject())));
    }
}
