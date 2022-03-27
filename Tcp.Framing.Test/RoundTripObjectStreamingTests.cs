using Xunit;

namespace Tcp.Framing.Test;
public class RoundTripObjectStreamingTests
{
    [Fact]
    public async Task TestSingleObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1235);

        Task.Run(async () => {
        IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
        await serverObjectStreamer.WriteObject(input);
        });

        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
        Assert.Equal(input, await clientObjectStreamer.ReadObject());
    }

    [Fact]
    public async Task TestMultiObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1236);

        Task.Run(() => {
            IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
            Enumerable.Range(0,100).ToList().ForEach(async i => await serverObjectStreamer.WriteObject(input));
        });
        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
        Enumerable.Range(0,100).ToList().ForEach(async i => Assert.True(input.Equals(await clientObjectStreamer.ReadObject())));
    }
}
