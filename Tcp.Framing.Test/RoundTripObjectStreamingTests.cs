using Xunit;
using System.Linq;

namespace Tcp.Framing.Test;
public class RoundTripObjectStreamingTests
{
    [Fact]
    public async Task TestSingleObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1235);

        Task receive = Task.Run(async () => {
        IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
        await serverObjectStreamer.WriteObjectAsync(input);
        });

        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
        Assert.Equal(input, await clientObjectStreamer.ReadObjectAsync());
        await receive;
    }

    [Fact]
    public async Task TestMultiObjectStreamingRoundTrip()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1236);

        Task receive = Task.Run(() => {
            IObjectStreamer<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
            Enumerable.Range(0,100).ToList().ForEach(async i => await serverObjectStreamer.WriteObjectAsync(input));
        });
        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);
        Enumerable.Range(0,100).ToList().ForEach(async i => Assert.True(input.Equals(await clientObjectStreamer.ReadObjectAsync())));
        await receive;
    }

    [Fact]
    public async Task TestMultiObjectStreamingEnumeration()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1243);

        Task receive = Task.Run(async () => {
            IObjectEnumerator<ExampleTestObject> serverObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ListenerStream);
            await serverObjectStreamer.WriteEnumerable(Enumerable.Range(0,100).Select(i => input), new CancellationTokenSource(500).Token);
        }, new CancellationTokenSource(500).Token);
        IObjectEnumerator<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);

        var output = await clientObjectStreamer.ReadAsyncEnumerable(new CancellationTokenSource(500).Token).Take(100).Select(a => a.ExampleInt).ToListAsync();
        Assert.Equal(100,output.Count);
    }
}
