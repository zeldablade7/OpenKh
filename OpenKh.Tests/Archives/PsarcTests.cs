using OpenKh.Common.Archives;
using Xunit;

namespace OpenKh.Tests.Archives
{
    public class PsarcTests
    {
        [Fact]
        public void IsAmountOfTocEntriesCorrect() => Helpers.UseAsset("test.psarc", stream =>
        {
            Psarc psarc = new Psarc(stream);
            Assert.Equal(3, psarc.Toc.Count);
        });

        [Fact]
        public void HasExpectedFile() => Helpers.UseAsset("test.psarc", stream =>
        {
            Psarc psarc = new Psarc(stream);
            Assert.True((psarc?.Toc[2]?.FileName ?? string.Empty) == "somefolder/somefile.bin");
        });
    }
}
