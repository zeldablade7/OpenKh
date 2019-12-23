//https://github.com/ColdSauce/psxsdk/blob/master/tools/vag2wav.c
//https://github.com/losnoco/vgmstream/blob/master/src/meta/vag.c
//https://gitlab.com/kenjiuno/khkh_xldM/blob/master/khiiMapv/ParseSD.cs
using System.Collections.Generic;
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
            if (Version == 4 && ChannelSize == FileSize - 0x60 && reader.ReadInt32Swap() != 0)
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

            reader.BaseStream.Position = 48 + 16 * Channels;

            wav = Decode(stream);
        }

        private Stream Decode(Stream stream)
        {
            //List<ushort> bytelist = new List<ushort>();
            //using (BinaryReader reader = new BinaryReader(stream))
            //{
            //    double num1 = 0.0;
            //    double num2 = 0.0;
            //    while (stream.Position + 16L <= stream.Length)
            //    {
            //        var buffer = reader.ReadBytes(16);
            //        var flags = buffer[1];

            //        if (flags == 7)
            //            break;

            //        byte predict_nr = buffer[0];
            //        int shift_factor = predict_nr & 15;
            //        predict_nr >>= 4;

            //        for (int i = 2; i < 16; i++)
            //        {
            //            int num6 = buffer[i];
            //            //int d = (num6 & 15) << 12;
            //            //if ((d & 32768) != 0)
            //            //    d |= -65536;
            //            //num2 = (d >> shift_factor) + num1 * f[predict_nr, 0] + num2 * f[predict_nr, 1];
            //            num2 = DecodeBlockA(num6, num1, num2, shift_factor, predict_nr);
            //            bytelist.Add((ushort)(num2 + 0.5));

            //            //int s = (num6 & 240) << 8;
            //            //if ((s & 32768) != 0)
            //            //    s |= -65536;
            //            //num1 = (s >> shift_factor) + num2 * f[predict_nr, 0] + num1 * f[predict_nr, 1];
            //            num1 = DecodeBlockB(num6, num2, num1, shift_factor, predict_nr);
            //            bytelist.Add((ushort)(num1 + 0.5));
            //        }
            //    }
            //}

            //MemoryStream ms = new MemoryStream();
            //int count = bytelist.Count;
            //BinaryWriter binaryWriter = new BinaryWriter(ms);
            //binaryWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
            //binaryWriter.Write(50 + (Channels * 2) * count);
            //binaryWriter.Write(Encoding.ASCII.GetBytes("WAVE"));
            //binaryWriter.Write(Encoding.ASCII.GetBytes("fmt "));
            //binaryWriter.Write(18);
            //binaryWriter.Write((short)1);
            //binaryWriter.Write((short)Channels);
            //binaryWriter.Write(SampleRate);
            //binaryWriter.Write(SampleRate * (Channels*2));
            //binaryWriter.Write((short)(Channels*2));
            //binaryWriter.Write((short)16);
            //binaryWriter.Write((short)0);
            //binaryWriter.Write(Encoding.ASCII.GetBytes("fact"));
            //binaryWriter.Write(4);
            //binaryWriter.Write(count);
            //binaryWriter.Write(Encoding.ASCII.GetBytes("data"));
            //binaryWriter.Write((Channels*2) * count);
            var bytelist = new List<ushort>();
            if (Channels == 1)
                bytelist = Decode1Channel(stream);
            else if (Channels == 2)
                bytelist = Decode2Channel(stream);

            var ms = BuildWavHeader(bytelist.Count);
            BinaryWriter writer = new BinaryWriter(ms);
            bytelist.ForEach(d => writer.Write(d));

            //foreach(var d in bytelist)
            //{
            //    binaryWriter.Write(d);
            //}

            return ms;
        }

        private Stream BuildWavHeader(int count)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(50 + (Channels * 2) * count);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(18);
                writer.Write((short)1);
                writer.Write((short)Channels);
                writer.Write(SampleRate);
                writer.Write(SampleRate * (Channels * 2));
                writer.Write((short)(Channels * 2));
                writer.Write((short)16);
                writer.Write((short)0);
                writer.Write(Encoding.ASCII.GetBytes("fact"));
                writer.Write(4);
                writer.Write(count);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write((Channels * 2) * count);
            return ms;
        }

        private double DecodeBlockA(int num6, double prevAdd, double inout, int shift_factor, int predict_nr)
        {
            int d = (num6 & 15) << 12;
            if ((d & 32768) != 0)
                d |= -65536;
            inout = (d >> shift_factor) + prevAdd * f[predict_nr, 0] + inout * f[predict_nr, 1];
            return inout;
        }

        private double DecodeBlockB(int num6, double prevAdd, double inout, int shift_factor, int predict_nr)
        {
            int d = (num6 & 240) << 8;
            if ((d & 32768) != 0)
                d |= -65536;
            inout = (d >> shift_factor) + prevAdd * f[predict_nr, 0] + inout * f[predict_nr, 1];
            return inout;
        }

        private List<ushort> Decode1Channel(Stream stream)
        {
            List<ushort> bytelist = new List<ushort>();
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

                    for (int i = 2; i < 16; i++)
                    {
                        int num6 = buffer[i];
                        num2 = DecodeBlockA(num6, num1, num2, shift_factor, predict_nr);
                        bytelist.Add((ushort)(num2 + 0.5));

                        num1 = DecodeBlockB(num6, num2, num1, shift_factor, predict_nr);
                        bytelist.Add((ushort)(num1 + 0.5));
                    }
                }
            }
            return bytelist;
        }
        private List<ushort> Decode2Channel(Stream stream)
        {
            int bytesLeft = ChannelSize - 16 * Channels;
            List<ushort> bytelist = new List<ushort>();
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

                    for (int i = 2; i < 16; i++)
                    {
                        int num6 = buffer[i];
                        int num7 = buffer[i + 16];
                        //int d1 = (num6 & 15) << 12;
                        //if ((d1 & 32768) != 0)
                        //    d1 |= -65536;
                        //num2 = (d1 >> shift_factor1) + num1 * f[predict_nr1, 0] + num2 * f[predict_nr1, 1];
                        num2 = DecodeBlockA(num6, num1, num2, shift_factor1, predict_nr1);
                        bytelist.Add((ushort)(num2 + 0.5));

                        //int d2 = (num7 & 15) << 12;
                        //if ((d2 & 32768) != 0)
                        //    d2 |= -65536;
                        //num4 = (d2 >> shift_factor2) + num3 * f[predict_nr2, 0] + num4 * f[predict_nr2, 1];
                        num4 = DecodeBlockA(num7, num3, num4, shift_factor2, predict_nr2);
                        bytelist.Add((ushort)(num4 + 0.5));

                        //int s1 = (num6 & 240) << 8;
                        //if ((s1 & 32768) != 0)
                        //    s1 |= -65536;
                        //num1 = (s1 >> shift_factor1) + num2 * f[predict_nr1, 0] + num1 * f[predict_nr1, 1];
                        num1 = DecodeBlockB(num6, num2, num1, shift_factor1, predict_nr1);
                        bytelist.Add((ushort)(num1 + 0.5));

                        //int s2 = (num7 & 240) << 8;
                        //if ((s2 & 32768) != 0)
                        //    s2 |= -65536;
                        //num3 = (s2 >> shift_factor2) + num4 * f[predict_nr2, 0] + num3 * f[predict_nr2, 1];
                        num3 = DecodeBlockB(num7, num4, num3, shift_factor2, predict_nr2);
                        bytelist.Add((ushort)(num3 + 0.5));
                    }
                }
            }
            //double s_1 = 0.0;
            //double s_2 = 0.0;
            //double s_3 = 0.0;
            //double s_4 = 0.0;
            //while (bytesLeft > 0U)
            //{
            //    byte[] buffer = reader.ReadBytes(32);
            //    bytesLeft -= 32;
            //    if (buffer.Length >= 32 && buffer[1] != 7 && buffer[17] != 7)
            //    {
            //        byte predict_nr1 = buffer[0];
            //        byte predict_nr2 = buffer[16];
            //        int shift_factor1 = predict_nr1 & 15;
            //        predict_nr1 >>= 4;
            //        int shift_factor2 = predict_nr2 & 15;
            //        predict_nr2 >>= 4;
            //        for (int i = 2; i < 16; ++i)
            //        {
            //            byte sam1 = buffer[i];
            //            byte sam2 = buffer[16 + i];
            //            int sam1_shift = (sam1 & 15) << 12;
            //            if ((sam1_shift & 32768) == 32768)
            //                sam1_shift |= -65536;
            //            s_2 = (sam1_shift >> shift_factor1) + s_1 * f[predict_nr1,0] + s_2 * f[predict_nr1,1];
            //            bytelist.Add((ushort)(s_2 + 0.5));

            //            int sam2_shift = (sam2 & 15) << 12;
            //            if ((sam2_shift & 32768) == 32768)
            //                sam2_shift |= -65536;
            //            s_4 = (sam2_shift >> shift_factor2) + s_3 * f[predict_nr2,0] + s_4 * f[predict_nr2,1];
            //            bytelist.Add((ushort)(s_4 + 0.5));

            //            int sam1_shift2 = (sam1 & 240) << 8;
            //            if ((sam1_shift2 & 32768) == 32768)
            //                sam1_shift2 |= -65536;
            //            s_1 = (sam1_shift2 >> shift_factor1) + s_2 * f[predict_nr1,0] + s_1 * f[predict_nr1,1];
            //            bytelist.Add((ushort)(s_1 + 0.5));

            //            int sam2_shift2 = (sam2 & 240) << 8;
            //            if ((sam2_shift2 & 32768) == 32768)
            //                sam2_shift2 |= -65536;
            //            s_3 = (sam2_shift2 >> shift_factor2) + s_4 * f[predict_nr2,0] + s_3 * f[predict_nr2,1];
            //            bytelist.Add((ushort)(s_3 + 0.5));
            //        }
            //        if (buffer[1] == 1 || buffer[17] == 1)
            //            break;
            //    }
            //    else
            //        break;
            //}
            return bytelist;
        }

    }
}
