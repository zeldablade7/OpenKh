//https://github.com/rmcolbert/PSARC.Net
//https://www.psdevwiki.com/ps3/PlayStation_archive_(PSARC)

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Common.Archives
{
    public class Psarc
    {
        public uint Version { get; }
        public uint CompressionMethod { get; }     // zlib or lzma
        public uint TocLength { get; }             // Includes Header
        public uint TocEntrySize { get; }          // Size of a single entry in the TOC, default is 30
        public uint TocEntries { get; }            // Total number of entries including the manifest
        public uint BlockSize { get; }             // The size of each block decompressed.
        public uint ArchiveFlags { get; }
        public List<TocEntry> Toc { get; }

        public class TocEntry
        {
            public byte[] MD5;                          // hash of FILENAME.
            public uint BlockListStart;
            public ulong OriginalSize;                  // UInt40 (5 Bytes)
            public ulong StartOffset;                   // UInt40 (5 Bytes)
            public string FileName;
        }

        private readonly uint MagicCode = 0x50534152;
        private BigEndianBinaryReader reader;

        public Psarc(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException("Read or seek must be supported.");

            reader = new BigEndianBinaryReader(stream);
            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            Toc = new List<TocEntry>();

            Version = reader.ReadUInt32();
            CompressionMethod = reader.ReadUInt32();
            TocLength = reader.ReadUInt32();
            TocEntrySize = reader.ReadUInt32();
            TocEntries = reader.ReadUInt32();
            BlockSize = reader.ReadUInt32();
            ArchiveFlags = reader.ReadUInt32();

            for (uint i = 0; i < TocEntries; i++)
            {
                Toc.Add(new TocEntry()
                {
                    MD5 = reader.ReadBytes(16),
                    BlockListStart = reader.ReadUInt32(),
                    OriginalSize = reader.ReadUInt40(),
                    StartOffset = reader.ReadUInt40()
                });
            }

            //Extract manifest file for filenames
            var manifest = DecompressFile(0);
            var filenames = new List<string>(Encoding.Default.GetString(manifest).Split('\n'));
            if (filenames.Count != TocEntries - 1)
                filenames = new List<string>(Encoding.Default.GetString(manifest).Split('\0'));
            filenames.Insert(0, "manifest.txt");

            for (int i = 0; i < TocEntries; i++)
            {
                Toc[i].FileName = filenames[i];
            }
        }

        private byte[] DecompressFile(int index)
        {
            if (index > (TocEntries - 1))
                return new byte[0];

            byte[] outputFile;
            reader.BaseStream.Position = (long)Toc[index].StartOffset;

            uint isZipped = reader.ReadUInt16();
            reader.BaseStream.Position -= 2;

            ulong zBlocks = (uint)(Math.Ceiling(Toc[index].OriginalSize / (double)BlockSize));

            if (isZipped == 0x78da || isZipped == 0x7801)
            {
                ulong fileSize = zBlocks * BlockSize;
                outputFile = Inflate(reader.ReadBytes((int)fileSize), (uint)zBlocks, BlockSize, Toc[index].OriginalSize);
            }
            else
                outputFile = reader.ReadBytes((int)Toc[index].OriginalSize);

            if (Toc[index].OriginalSize != (ulong)outputFile.LongLength)
                throw new InvalidDataException($"Expected size: {Toc[index].OriginalSize}, Actual size: {outputFile.LongLength}");

            return outputFile;
        }

        private byte[] Inflate(byte[] compressedStream, uint zBlocks, uint blockSize, ulong fileSize)
        {
            byte[] uncompressed = new byte[fileSize];
            using (MemoryStream stream = new MemoryStream(compressedStream))
            using (InflaterInputStream inflater = new InflaterInputStream(stream))
                inflater.Read(uncompressed, 0, (int)fileSize);

            return uncompressed;
        }
    }
}
