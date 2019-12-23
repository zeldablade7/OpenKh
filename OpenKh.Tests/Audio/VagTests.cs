using OpenKh.Audio;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Audio
{
    public class VagTests
    {
        private void WriteWavToFile(Stream stream, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                stream.Position = 0L;
                stream.CopyTo(fs);
                fs.Flush();
            }
        }

        [Fact]
        public void DecodeTest() => Common.FileOpenRead(@"E:\HAX\KH Hacking\KH2FM Toolkit\export_raw_new\KH2\voice\fm\event\al10101ia.vag", stream =>
        {
            var vag = new Vag(stream);
            WriteWavToFile(vag.WaveStream, @"d:\out.wav");
        });

        [Fact]
        public void Decode2ch() => Common.FileOpenRead(@"E:\HAX\KH Hacking\KH2FM Toolkit\export_raw_new\KH2\vagstream\End_Piano.vas", stream =>
        {
            var vag = new Vag(stream);
            WriteWavToFile(vag.WaveStream, @"d:\out2ch.wav");
        });

        [Fact]
        public void VsbTest() => Common.FileOpenRead(@"E:\HAX\KH Hacking\KH2FM Toolkit\export_raw_new\KH2\voice\us\battle\al0_aladdin.vsb", stream =>
        {
            var vags = Vsb.Read(stream);
            int count = 0;
            foreach(var vag in vags)
            {
                WriteWavToFile(vag.WaveStream, $@"d:\{count}.wav");
                ++count;
            }
        });

        [Fact]
        public void WdTest() => Common.FileOpenRead(@"E:\HAX\KH Hacking\KH2FM Toolkit\export_raw_new\KH2\se\wave0000.wd", stream =>
        {
            var vags = Wd.Read(stream);
            int count = 0;
            foreach (var vag in vags)
            {
                WriteWavToFile(vag.WaveStream, $@"d:\{count}.wav");
                ++count;
            }
        });
    }
}
