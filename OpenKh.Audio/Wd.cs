//https://gitlab.com/kenjiuno/khkh_xldM/blob/master/khiiMapv/ParseSD.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.IO;

namespace OpenKh.Audio
{
    public static class Wd
    {
        private struct Entry
        {
            public int Offset, Length, Instrument, SamplesPerSecond;
        }

        private static readonly byte[] MagicCode = { 0x57, 0x44 };

        public static IEnumerable<Vag> Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            var test = reader.ReadBytes(2);
            if (stream.Length < 16L || !MagicCode.SequenceEqual(test))
                throw new InvalidDataException("Invalid header");

            reader.BaseStream.Position = 0x8;
            var entries = new List<Entry>();
            int instrumentCount = reader.ReadInt32();
            int num1 = reader.ReadInt32();

            reader.BaseStream.Position = 0x20;
            for (int i = 0; i < instrumentCount; i++)
            {
                var num4 = 32 + 4 * (instrumentCount + 3 & -4) + 32 * i;
                reader.BaseStream.Position = num4 + 16;

                if (reader.ReadInt64() != 0L || reader.ReadInt64() != 0L)
                {
                    reader.BaseStream.Position = num4 + 4;
                    var ent = new Entry
                    {
                        Offset = reader.ReadInt32(),
                        Instrument = i
                    };
                    reader.BaseStream.Position = num4 + 22;
                    ent.SamplesPerSecond = reader.ReadUInt16();
                    
                    num4 = 32 + 16 * (instrumentCount + 3 & -4) + 32 * num1 + ent.Offset;
                    stream.Position = num4;
                    while (stream.Position < stream.Length)
                    {
                        var buffer = reader.ReadBytes(16);
                        var index2 = 0;
                        while (index2 < 16 && buffer[index2] == 0)
                            ++index2;
                        if (index2 == 16)
                            break;
                    }
                    ent.Offset = num4;
                    ent.Length = Convert.ToInt32(stream.Position) - num4 - 32;
                    entries.Add(ent);
                }
            }

            return entries
                .Select(x => new Vag(new SubStream(stream, x.Offset, x.Length), 1, x.SamplesPerSecond))
                .ToList();
        }
    }
}
