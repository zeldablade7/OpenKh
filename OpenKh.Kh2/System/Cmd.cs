using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class Cmd
    {
        [Data] public ushort Id { get; set; }
        [Data] public ushort Unk02 { get; set; }
        [Data] public short Unk04 { get; set; }
        [Data] public byte SubMenuId { get; set; }
        [Data] public byte IconId { get; set; }
        [Data] public int TextId { get; set; }
        [Data] public int Unk0C { get; set; }
        //[Data] public ushort Unk10 { get; set; }
        [Data(Count = 0x20)] public byte[] Unk10 { get; set; }
    }
}
