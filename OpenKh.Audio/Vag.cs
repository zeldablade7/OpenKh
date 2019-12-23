//https://github.com/ColdSauce/psxsdk/blob/master/tools/vag2wav.c
//https://github.com/losnoco/vgmstream/blob/master/src/meta/vag.c
//https://gitlab.com/kenjiuno/khkh_xldM/blob/master/khiiMapv/ParseSD.cs
using System.IO;
using System.Text;
using Xe.IO;

namespace OpenKh.Audio
{
    public class Vag : IAudio
    {
        private const uint MagicCode = 0x70474156U;
        private static readonly double[,] f = new double[5, 2]
        {
            { 0.0, 0.0 },
            { 60.0 / 64.0,  0.0 },
            { 115.0 / 64.0, -52.0 / 64.0 },
            { 98.0 / 64.0, -55.0 / 64.0 },
            { 122.0 / 64.0, -60.0 / 64.0 }
        };

        public int Version { get; }
        public long FileSize { get; }
        public int ChannelSize { get; }
        public int SampleRate { get; }
        public byte Channels { get; }
        public byte Volume { get; }
        public string StreamName { get; }

        public bool IsLoopable { get; }
        public int LoopStartSample { get; }
        public int LoopEndSample { get; }

        public Stream wav { get; }

        public Vag(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            FileSize = stream.Length;

            Version = reader.ReadInt32Swap();

            reader.BaseStream.Position = 0xC;

            ChannelSize = reader.ReadInt32Swap();
            SampleRate = reader.ReadInt32Swap();

            reader.BaseStream.Position = 0x1C;
            if (Version == 4 && (ChannelSize == FileSize - 0x60) && reader.ReadInt32Swap() != 0)
            {
                //KH VAG / VAS file
                //VAGs converted from MFAudio have Version 3
                reader.BaseStream.Position = 0x14;
                LoopStartSample = reader.ReadInt32Swap();
                LoopEndSample = reader.ReadInt32Swap();
                IsLoopable = (LoopEndSample > 0);

                reader.BaseStream.Position = 0x1E;
                Channels = reader.ReadByte();
                Volume = reader.ReadByte();
            }
            else
                Channels = 1;

            reader.BaseStream.Position = 0x20;
            StreamName = Encoding.UTF8.GetString(reader.ReadBytes(0x10)).Replace("\0", string.Empty);

            reader.BaseStream.Position = 48 + 16 * Channels;
            wav = Decode(stream);
        }

        private Stream Decode(Stream stream)
        {
            var wavstream = Channels == 2 ? GetTwoChannelWavStream(stream) : GetOneChannelWavStream(stream);
            var wavheader = AudioHelpers.BuildWavHeader((int)wavstream.Length, Channels, SampleRate);
            wavheader.Position = wavheader.Length;
            wavstream.Position = 0;
            wavstream.CopyTo(wavheader);

            return wavheader;
        }

        private Stream GetOneChannelWavStream(Stream stream)
        {
            MemoryStream outStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(outStream);
            using (BinaryReader reader = new BinaryReader(stream))
            {
                double num1 = 0.0;
                double num2 = 0.0;
                while (stream.Position + 16L <= stream.Length)
                {
                    var buffer = reader.ReadBytes(16);
                    var flags = buffer[1];

                    if (flags == 7)
                        break;

                    byte predict_nr = buffer[0];
                    int shift_factor = predict_nr & 15;
                    predict_nr >>= 4;

                    for (int i = 2; i < 16; ++i)
                    {
                        int num6 = buffer[i];
                        num2 = DecodeBufferA(num6, num1, num2, shift_factor, predict_nr);
                        writer.Write((ushort)(num2 + 0.5));

                        num1 = DecodeBufferB(num6, num2, num1, shift_factor, predict_nr);
                        writer.Write((ushort)(num1 + 0.5));
                    }
                }
            }
            return outStream;
        }
        private Stream GetTwoChannelWavStream(Stream stream)
        {
            MemoryStream outStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(outStream);
            using (BinaryReader reader = new BinaryReader(stream))
            {
                double num1 = 0.0;
                double num2 = 0.0;
                double num3 = 0.0;
                double num4 = 0.0;
                while (stream.Position + 32L <= stream.Length)
                {
                    var buffer = reader.ReadBytes(32);
                    var flags1 = buffer[1];
                    var flags2 = buffer[17];

                    if (flags1 == 7 || flags2 == 7)
                        break;

                    byte predict_nr1 = buffer[0];
                    byte predict_nr2 = buffer[16];
                    int shift_factor1 = predict_nr1 & 15;
                    predict_nr1 >>= 4;
                    int shift_factor2 = predict_nr2 & 15;
                    predict_nr2 >>= 4;

                    for (int i = 2; i < 16; ++i)
                    {
                        int num6 = buffer[i];
                        int num7 = buffer[i + 16];

                        num2 = DecodeBufferA(num6, num1, num2, shift_factor1, predict_nr1);
                        writer.Write((ushort)(num2 + 0.5));

                        num4 = DecodeBufferA(num7, num3, num4, shift_factor2, predict_nr2);
                        writer.Write((ushort)(num4 + 0.5));

                        num1 = DecodeBufferB(num6, num2, num1, shift_factor1, predict_nr1);
                        writer.Write((ushort)(num1 + 0.5));

                        num3 = DecodeBufferB(num7, num4, num3, shift_factor2, predict_nr2);
                        writer.Write((ushort)(num3 + 0.5));
                    }
                }
            }
            return outStream;
        }

        private double DecodeBufferA(int i, double prevAdd, double inout, int shift_factor, int predict_nr)
        {
            int d = (i & 15) << 12;
            if ((d & 32768) != 0)
                d |= -65536;
            inout = (d >> shift_factor) + prevAdd * f[predict_nr, 0] + inout * f[predict_nr, 1];
            return inout;
        }

        private double DecodeBufferB(int i, double prevAdd, double inout, int shift_factor, int predict_nr)
        {
            int d = (i & 240) << 8;
            if ((d & 32768) != 0)
                d |= -65536;
            inout = (d >> shift_factor) + prevAdd * f[predict_nr, 0] + inout * f[predict_nr, 1];
            return inout;
        }
    }
}
