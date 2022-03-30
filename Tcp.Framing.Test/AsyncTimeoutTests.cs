using Xunit;

namespace Tcp.Framing.Test;

public class AsyncTimeoutTests
{
    [Fact]
    public async Task ConfirmAsyncEnumerableReadsAcceptCancellation()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1244);
        IObjectEnumerator<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);

        var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(500);//500ms

        await Assert.ThrowsAsync<OperationCanceledException>(async () => 
            await clientObjectStreamer.ReadAsyncEnumerable(cancellationSource.Token).Take(1).ToListAsync()
        );
    }

    [Fact]
    public async Task ConfirmAsyncReadAcceptsCancellation()
    {
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1245);
        IObjectStreamer<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);

        var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(500);//500ms

        await Assert.ThrowsAsync<OperationCanceledException>(async () => 
            await clientObjectStreamer.ReadObjectAsync(cancellationSource.Token)
        );
    }

    [Fact]
    public async Task ConfirmWriteEnumarableCancellation()
    {
        ExampleTestObject input = new ExampleTestObject(){ExampleDouble = 1.609344, ExampleInt = 42, ExampleString = "Nice"};
        var streamPair = TestNetworkStreamPairBuilder.GetTestStreamPair(1246);
        IObjectEnumerator<ExampleTestObject> clientObjectStreamer = new ObjectStreamer<ExampleTestObject>(streamPair.ClientStream);

        var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(500);

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await clientObjectStreamer.WriteEnumerable(Enumerable.Range(0,100).Select(i => input), cancellationSource.Token)
        );
    }

}