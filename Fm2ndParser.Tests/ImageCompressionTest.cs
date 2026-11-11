using Fm2ndParser.Utility;

namespace Fm2ndParser.Tests
{
    public class ImageCompressionTest
    {
        [Fact]
        public void CompressShouldReturnTheSameResultAsOriginalAlgorithm()
        {
            var unpackedImge = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "unpackedImge.dat"));
            var expectedPackedImge = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "expectedPackedImge.dat"));

            var packedImage = Rle3CodecCompression.Compress(unpackedImge);

            Assert.Equal(expectedPackedImge, packedImage);
        }
    }
}
