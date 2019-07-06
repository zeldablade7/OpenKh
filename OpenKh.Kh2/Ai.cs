using OpenKh.Common;
using System;
using System.IO;

namespace OpenKh.Kh2
{
    public class Ai
    {
        private const int THIS = 0;
        private const int UNKNOWN = -1;
        private readonly byte[] data;
        private int sp54;
        private int sp50;
        private int _08;
        private int _0c;
        private int _10;

        public string Name { get; }
        public int ProgramCounter { get; set; }
        public short LastOpcode { get; private set; }
        public bool Return { get; private set; }

        public Ai(Stream stream)
        {
            Name = stream.ReadString(0x10);
            data = stream.ReadBytes();
            ProgramCounter = data.ToInt(0x18);
        }

        public int GetProgram(int programId)
        {
            for (var programIndex = 0; ; programIndex++)
            {
                var id = data.ToInt(0xc + programIndex * 8 + 0);
                if (id == 0)
                    return 0;
                if (id == programId)
                    return data.ToInt(0xc + programIndex * 8 + 4);
            }
        }

        public void Tick()
        {
            var opcode = LastOpcode = ReadNext();
            int t7 = opcode & 0xF;

            switch (t7)
            {
                case 0:
                    func0(opcode);
                    break;
                case 8:
                    // This become an index to a certain int array.
                    var t2 = opcode >> 6;
                    sp54 = ReadNext();
                    // ????
                    break;
                default:
                    break;
            }

        }

        private void func0(int opcode)
        {
            var t5 = opcode >> 4 & 3;
            var t7 = t5 < 3;
            if (t5 != 2)
            {

            }
            else
                loc_1DA6B8(opcode);
        }

        private void loc_1DA6B8(int opcode)
        {
            sp50 = sub_1DA4D8(THIS, opcode);
            loc_1DA6A8();

        }

        private void loc_1DA6A8()
        {
            sub_1DA3D8(THIS, UNKNOWN);
        }

        private void sub_1DA3D8(int THIS, int pa1)
        {
            var t7 = _0c;
            // [_0c++] = *pa1

        }

        private int sub_1DA4D8(int THIS, int opcode)
        {
            var a1 = opcode >> 6;
            var t5 = ReadNext();
            if (a1 == 1)
            {
                return _10 + t5;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private short ReadNext()
        {
            return data.ToShort(ProgramCounter++ * 2);
        }
    }
}
