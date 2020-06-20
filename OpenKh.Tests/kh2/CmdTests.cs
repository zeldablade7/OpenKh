using Xunit;
using OpenKh.Common;
using OpenKh.Kh2.System;
using OpenKh.Kh2;
using System.Linq;

namespace OpenKh.Tests.kh2
{
    public class CmdTests
    {
        [Fact]
        public void Check() => Common.FileOpenRead(@"kh2/res/cmd.bin", x => x.Using(stream =>
        {
            var table = BaseTable<Cmd>.Read(stream);
            var grouped = table.Items.GroupBy(x => x.Unk0C).ToList();
            Assert.Equal(0x2EF, table.Count);
        }));
    }
}
