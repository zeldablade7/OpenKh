using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2
{
    public class Ai
    {
        private const int THIS = 0;
        private const int UNKNOWN = -1;
        private const int AiStackLength = 0x100;

        private readonly byte[] data;
        private int sp54;
        private object sp50;
        private IFoo spC0;
        private IFoo _10;
        private int _1c;
        private int _20;
        private int _24;
        private int _2c;
        private int _30;

        public Stack<int> StackPointer08 { get; } // 8($s2)
        public IFoo StackPointer0c { get; } // c($s2)
        public string Name { get; }
        public int ProgramCounter { get; set; }
        public short LastOpcode { get; private set; }
        public bool Return { get; private set; }

        public Ai(Stream stream)
        {
            StackPointer08 = new Stack<int>(AiStackLength);
            StackPointer0c = new FooStack(new Stack<int>(AiStackLength));
            _1c = 0;
            _20 = 0;
            _24 = 0;
            _2c = 0;
            _30 = 0;

            Name = stream.ReadString(0x10);
            data = stream.ReadBytes();
            ProgramCounter = GetProgram(3);
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
                case 1:
                    func1(opcode);
                    break;
                case 8:
                    // This become an index to a certain int array.
                    var t2 = opcode >> 6;
                    sp54 = ReadNext();
                    // ????
                    break;
                case 9:
                    func9(opcode);
                    break;
                default:
                    break;
            }

        }

        private void func0(short opcode)
        {
            var t5 = opcode >> 4 & 3;
            var t7 = t5 < 3;
            if (t5 != 2)
            {

            }
            else
                loc_1DA6B8(opcode);
        }

        private void func1(short opcode)
        {
            spC0 = sub_1DA4D8(THIS, opcode);
            loc_1DA6EC(THIS, spC0);
        }

        private void func9(short opcode)
        {
            var a1 = opcode >> 6;
            switch (a1)
            {
                case 0:
                    throw new NotImplementedException($"func9 {a1}");
                case 1:
                    throw new NotImplementedException($"func9 {a1}");
                case 2:
                    opcode9_switch2();
                    break;
                case 3:
                    throw new NotImplementedException($"func9 {a1}");
                case 4:
                    throw new NotImplementedException($"func9 {a1}");
                case 5:
                    throw new NotImplementedException($"func9 {a1}");
                case 6:
                    throw new NotImplementedException($"func9 {a1}");
                case 7:
                    throw new NotImplementedException($"func9 {a1}");
                case 8:
                    throw new NotImplementedException($"func9 {a1}");
                case 9:
                    throw new NotImplementedException($"func9 {a1}");
                default:
                    throw new NotImplementedException($"func9 default");
            }
        }

        private void opcode9_switch2()
        { // COMPLETE!
            ProgramCounter = StackPointer08.Pop();
            var stackedElements = StackPointer08.Pop();
            if (stackedElements < 2)
            {
                // This should never happen... only used to
                // replicate the same bugs that are in the
                // original game engine.
                if (stackedElements == 1)
                {
                    // Re-push the last popped element
                    StackPointer08.Push(stackedElements);
                }
                else if(stackedElements == 0)
                {
                    // Re-push last and second last popped elements
                    StackPointer08.Push(stackedElements);
                    StackPointer08.Push(ProgramCounter);
                }
            }

            stackedElements -= 2;
            while (stackedElements-- > 0)
                StackPointer08.Pop();
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

        private void loc_1DA6EC(int THIS, IFoo a1)
        {
            sub_1DA3F8(THIS, a1);
            loc_1DA62C();
        }

        private void loc_1DA62C()
        {
            throw new NotImplementedException();
        }

        private void sub_1DA3F8(int THIS, IFoo a1)
        {
            a1.Set(StackPointer0c.PopAndPeek());
        }

        private void sub_1DA3D8(int THIS, int pa1)
        {
            var t7 = StackPointer0c;
            // [_0c++] = *pa1

        }

        private IFoo sub_1DA4D8(int THIS, int opcode)
        {
            var a1 = opcode >> 6;
            var t5 = ReadNext();

            IFoo result;
            if (a1 == 1)
            {
                // loc_1DA54C
                result = _10;
            }
            else
            {
                // loc_1DA504
                if (a1 < 2)
                {
                    // loc_1DA510
                    if (a1 == 0)
                    {
                        // locret_1DA51C
                        result = new FooStack(StackPointer08);
                    }
                    else
                    {
                        result = new FooNull();
                    }
                }
                else
                {
                    // loc_1DA524
                    var t7 = 2;
                    throw new NotImplementedException("loc_1DA524");
                }
            }

            result.AddOffset(t5);
            return result;
        }

        private short ReadNext()
        {
            return data.ToShort(ProgramCounter++ * 2);
        }
    }

    public interface IFoo
    {
        int Peek();
        int Pop();
        int PopAndPeek();
        void Set(int value);
        void AddOffset(int offset);
    }

    public class FooNull : IFoo
    {
        public void AddOffset(int offset)
        {
            throw new NotImplementedException();
        }

        public int Peek()
        {
            throw new NotImplementedException();
        }

        public int Pop()
        {
            throw new NotImplementedException();
        }

        public int PopAndPeek()
        {
            throw new NotImplementedException();
        }

        public void Set(int value)
        {
            throw new NotImplementedException();
        }
    }

    public class FooStack : IFoo
    {
        private readonly Stack<int> _stack;

        public FooStack(Stack<int> stack)
        {
            _stack = stack;
        }

        public int Peek() => _stack.Peek();

        public int Pop() => _stack.Pop();

        public int PopAndPeek()
        {
            Pop();
            return Peek();
        }

        public void Set(int value)
        {
            Pop();
            _stack.Push(value);
        }

        public void AddOffset(int offset)
        {

        }
    }
}
