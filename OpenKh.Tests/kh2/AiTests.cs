using OpenKh.Kh2;
using OpenKh.Common;
using Xunit;
using System.IO;
using System.Linq;

namespace OpenKh.Tests.kh2
{
    public class AiTests
    {
        private const string FilePath = @"kh2/res/ms_b.bin";

        [Fact]
        public void ReadTheScriptName() =>
            Common.FileOpenRead(FilePath, myStream => myStream.Using(stream =>
            {
                var ai = new Ai(stream);
                Assert.Equal("ms_boss", ai.Name);
            }));

        [Fact]
        public void SetTheProgramCounterToTheStart() =>
            Common.FileOpenRead(FilePath, myStream => myStream.Using(stream =>
            {
                var ai = new Ai(stream);
                Assert.Equal(0x43, ai.ProgramCounter);
            }));

        [Theory]
        [InlineData(0x0A, 0x12)]
        [InlineData(0x03, 0x43)]
        [InlineData(0x1234, 0)]
        public void GetProgramCorrectly(int programId, int expectedProgramCounter) =>
            Common.FileOpenRead(FilePath, myStream => myStream.Using(stream =>
            {
                var ai = new Ai(stream);
                Assert.Equal(expectedProgramCounter, ai.GetProgram(programId));
            }));

        [Fact]
        public void TestOpcode1()
        {
            var ai = CreateAi(0x01, 0x00, 0x00, 0x00);
            var prevPc = ai.ProgramCounter;

            ai.Tick();

            Assert.Equal(prevPc + 2, ai.ProgramCounter);
        }

        [Fact]
        public void TestOpcode9_Switch2()
        {
            var ai = CreateAi(0x0089);
            ai.StackPointer08.Push(2);
            ai.StackPointer08.Push(1234);

            ai.Tick();

            Assert.Empty(ai.StackPointer08);
            Assert.Equal(1234, ai.ProgramCounter);
        }

        [Fact]
        public void ExecuteScript() =>
            Common.FileOpenRead(FilePath, myStream => myStream.Using(stream =>
            {
                var ai = new Ai(stream);
                ai.ProgramCounter = 0x43;

                ai.Tick();
                Assert.Equal(0x45, ai.ProgramCounter);

                ai.Tick();
                Assert.Equal(0x47, ai.ProgramCounter);

                ai.Tick();
            }));

        [Fact]
        public void ExpectCorrectPcThroughRun() =>
            Common.FileOpenRead(FilePath, myStream => myStream.Using(stream =>
            {
                var ai = new Ai(stream);
                Expect(ai, 0x43, 0x60);
                Expect(ai, 0x45, 0x108);
                Expect(ai, 0x48, 0x1);
                Expect(ai, 0x4a, 0x30);
                Expect(ai, 0x4c, 0x108);
                Expect(ai, 0x97, 0x1);
                Expect(ai, 0x99, 0x0);
                Expect(ai, 0x9c, 0x81);
                Expect(ai, 0x9e, 0x80);
                Expect(ai, 0xa1, 0x81);
                Expect(ai, 0xa3, 0x89);
                Expect(ai, 0x4e, 0x30);
                Expect(ai, 0x50, 0x108);
                Expect(ai, 0xa4, 0x1);
                Expect(ai, 0xa6, 0xb0);
                Expect(ai, 0xa8, 0x80);
                Expect(ai, 0xab, 0x46);
                Expect(ai, 0xac, 0x205);
                Expect(ai, 0xad, 0x89);
                Expect(ai, 0x52, 0x47);
                Expect(ai, 0x54, 0x9);
                Assert.True(ai.Return);
            }));

        private void Expect(Ai ai, int expectedPc, int expectedOpcode)
        {
            Assert.Equal(expectedPc, ai.ProgramCounter);
            Assert.False(ai.Return);
            ai.Tick();
            Assert.Equal(expectedOpcode, ai.LastOpcode);
        }

        private Ai CreateAi(params byte[] program)
        {
            var head = new byte[]
            {
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x00
            };
            var unknown = new byte[]
            {
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
            };
            var functions = new byte[]
            {
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
            };

            return new Ai(new MemoryStream(head.Concat(unknown).Concat(functions).Concat(program).ToArray()))
            {
                ProgramCounter = (unknown.Length + functions.Length) / 2
            };
        }
    }
}
