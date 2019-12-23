using System.IO;

namespace OpenKh.Audio
{
    public static class AudioHelpers
    {
        public static Stream BuildWavHeader(int sampleCount, int channels, int sampleRate)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(0x46464952);                   //RIFF
            writer.Write(50 + (channels * 2) * sampleCount);
            writer.Write(0x45564157);                   //WAVE
            writer.Write(0x20746D66);                   //fmt
            writer.Write(18);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * (channels * 2));
            writer.Write((short)(channels * 2));
            writer.Write((short)16);
            writer.Write((short)0);
            writer.Write(0x74636166);                   //fact
            writer.Write(4);
            writer.Write(sampleCount);
            writer.Write(0x61746164);                   //data
            writer.Write((channels * 2) * sampleCount);
            return ms;
        }
    }
}
