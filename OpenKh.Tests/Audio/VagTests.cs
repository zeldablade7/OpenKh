using OpenKh.Audio;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Audio
{
    public class VagTests
    {
        [Fact]
        public void DecodeTest() => Common.FileOpenRead(@"E:\HAX\KH Hacking\KH2FM Toolkit\export_raw_new\KH2\voice\fm\event\al10101ia.vag", stream =>
        {
            var vag = new Vag(stream);
            using (FileStream fs = new FileStream(@"d:\out.wav", FileMode.OpenOrCreate))
            {
                vag.wav.Position = 0L;
                vag.wav.CopyTo(fs);
                fs.Flush();
            }
        });

        [Fact]
        public void Decode2ch() => Common.FileOpenRead(@"E:\HAX\KH Hacking\KH2FM Toolkit\export_raw_new\KH2\vagstream\End_Piano.vas", stream =>
        {
            var vag = new Vag(stream);
            using (FileStream fs = new FileStream(@"d:\out2ch.wav", FileMode.OpenOrCreate))
            {
                vag.wav.Position = 0L;
                vag.wav.CopyTo(fs);
                fs.Flush();
            }
        });
    }
}
