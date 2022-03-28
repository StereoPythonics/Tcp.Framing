using Xunit;
namespace Tcp.Framing.Test;

public class RoundTripCompressionTests
{
    [Fact]
    public void TestGzipCompression()
    {
        GZipedJsonSerializer<ExampleTestObject> serializer = new Framing.GZipedJsonSerializer<ExampleTestObject>();
        ExampleTestObject testInput = new ExampleTestObject(){ExampleString = "It works!, how nice!"};
        ExampleTestObject output = serializer.Deserialize(serializer.Serialize(testInput));

        Assert.Equal(testInput,output);
    }
}