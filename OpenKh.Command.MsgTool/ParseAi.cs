using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ParseAI
{
    public class Parse03
    {
        MemoryStream si;
        BinaryReader br;
        TextWriter wri;

        public Parse03(TextWriter wri)
        {
            this.wri = wri;
        }

        public void Run(byte[] bin)
        {
            si = new MemoryStream(bin, false);
            br = new BinaryReader(si);

            {
                if (si.Length < 1) return;

                si.Position = 0;
                wri.WriteLine("#Parseai 20130512");
                wri.WriteLine("#Header: {0}", Ut.Read0Str(br));

                if (si.Length < 16 + 12) return;

                si.Position = 16;
                int v0 = br.ReadInt32();
                int v4 = br.ReadInt32();
                int v8 = br.ReadInt32();
                wri.WriteLine("#Prefix: {0:x} {1:x} {2:x}", v0, v4, v8);
            }

            for (int x = 0; ; x++)
            {
                si.Position = 0x1c + 8 * x;
                int key = br.ReadInt32();
                int off = br.ReadInt32();
                if (key == 0 && off == 0) break;
                wri.WriteLine("#Trigger: K{0} {1}", key, off);
            }

            wri.WriteLine("#Format: label: ai-code ; ai-decimal-off real-hex-off ");

            for (int x = 0; ; x++)
            {
                si.Position = 0x1c + 8 * x;
                int key = br.ReadInt32();
                int off = br.ReadInt32();
                if (key == 0 && off == 0) break;
                Walk(key, off);
            }

            Gen();
        }

        private void Gen()
        {
            int cx = dict.Keys.Max();
            int entire = cx;
            if (labels.Count != 0) entire = Math.Max(entire, labels.Keys.Max());
            if (errors.Count != 0) entire = Math.Max(entire, errors.Keys.Max());
            for (int x = 0; x <= entire; x++)
            {
                string s;
                if (labels.TryGetValue(x, out s))
                {
                    wri.WriteLine("{0}:", s);
                }
                if (errors.ContainsKey(x))
                {
                    wri.WriteLine(" ; Can't reach here!");
                }
                Dis o;
                if (dict.TryGetValue(x, out o))
                {
                    wri.WriteLine("{0,-40}; {1:x} {2:x}", o.Desc, x, 2 * x);

                    if (x < cx)
                    {
                        int x1 = x + o.Len / 2;
                        if (dict.ContainsKey(x1) == false)
                        {
                            int x2 = dict.Keys.First(p => p > x1);
                            if (x2 - x1 > 1)
                            {
                                wri.WriteLine(" ; Unscanned {2} words. {0} to {1}", x1, x2 - 1, x2 - x1);
                            }
                        }
                    }
                }
            }
        }

        SortedDictionary<int, Dis> dict = new SortedDictionary<int, Dis>();
        SortedDictionary<int, String> labels = new SortedDictionary<int, string>();
        SortedDictionary<int, object> errors = new SortedDictionary<int, object>();

        class Dis
        {
            public int Len = 0;
            public String Desc = String.Empty;

            public Dis(int cb, String s)
            {
                this.Len = cb;
                this.Desc = s;
            }
        }

        class Ut
        {
            public static string Read0Str(BinaryReader br)
            {
                String s = "";
                while (true)
                {
                    int v = br.ReadByte();
                    if (v == 0)
                        break;
                    s += (char)v;
                }
                return s;
            }
        }

        class CmdObs
        {
            public void Eat(int v0)
            {
                if (0xE0 == (255 & v0))
                {
                    stat = 0;
                }
                else if (0xC0 == (255 & v0) && stat >= 0)
                {
                    stat++;
                }
                else
                {
                    stat = -1;
                }
            }

            public T Curt
            {
                get
                {
                    switch (stat)
                    {
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            return T.Label;
                    }
                    return T.Val;
                }
            }

            int stat = -1;

            public enum T
            {
                Val, Label,
            }
        }

        private void Walk(int key, int off)
        {
            Queue<int> offq = new Queue<int>();
            offq.Enqueue(off);
            labels[off] = "K" + key;
            CmdObs obs = new CmdObs();
            while (offq.Count != 0)
            {
                int nextoff = offq.Dequeue();
                while (true)
                {
                    off = nextoff;

                    if (dict.ContainsKey(off)) break;

                    si.Position = 0x10 + off * 2;

                    if (si.Position >= si.Length)
                    {
                        errors[off] = null;
                        break; // 仕方なし
                    }

                    try
                    {
                        int v0 = br.ReadUInt16();
                        int cm4 = v0 & 15;
                        int cm8 = v0 & 255;

                        obs.Eat(v0);

                        if (false) { }
                        // -- 16 bits cmd
                        else if (v0 == 0xffff)
                        {
                            dict[off] = new Dis(2, String.Format("TERM"));
                            break;
                        }
                        // -- 8 bits cmd
                        else if (cm8 == 0x30)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmd30 {0:x2} {1:x2} {2:x2} ", v0 >> 8, v2, v3));
                            nextoff = off + 2;
                        }
                        else if (cm8 == 0x60)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmd60 {0:x2} {1:x2} {2:x2} ", v0 >> 8, v2, v3));
                            nextoff = off + 2;
                        }
                        else if (cm8 == 0xa0)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmda0 {0:x2} {1:x2} {2:x2} ", v0 >> 8, v2, v3));
                            nextoff = off + 2;
                        }
                        else if (cm8 == 0xc0)
                        {
                            int v2 = br.ReadInt32();
                            if (obs.Curt == CmdObs.T.Label && v2 != 0)
                            {
                                int newoff = v2;
                                offq.Enqueue(newoff);
                                dict[off] = new Dis(6, String.Format("Cmdc0l {0:x2} {1} ", v0 >> 8, GenLabel(newoff)));
                            }
                            else
                            {
                                dict[off] = new Dis(6, String.Format("Cmdc0i {0:x2} {1:x8} ", v0 >> 8, v2));
                            }
                            nextoff = off + 3;
                        }
                        else if (cm8 == 0xe0)
                        {
                            int v2 = br.ReadUInt16();
                            si.Position = 0x10 + v2 * 2;
                            dict[off] = new Dis(4, String.Format("Print {0:x2} '{1}' ", v0 >> 8, Ut.Read0Str(br)));
                            nextoff = off + 2;
                        }
                        else if (cm8 == 0x0B)
                        {
                            int v2 = br.ReadInt32();
                            int newoff = off + 3 + v2;
                            dict[off] = new Dis(6, String.Format("Call {0:x3} {1} ; {2} ", v0 >> 4, GenLabel(newoff), v2));
                            offq.Enqueue(newoff);
                            nextoff = off + 3;
                        }
#if false
                        else if (cm8 == 0x01) {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmd01 {0:x2} {1:x2} {2:x2} ", v0 >> 8, v2, v3));
                            nextoff = off + 2;
                        }
#endif
                        else if (cm8 == 0x00)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            int v4 = br.ReadByte();
                            int v5 = br.ReadByte();
                            dict[off] = new Dis(6, String.Format("Cmd00 {0:x2} {1:x2} {2:x2} {3:x2} {4:x2} ", v0 >> 8, v2, v3, v4, v5));
                            nextoff = off + 3;
                        }
#if false
                        else if (cm8 == 0x0a) {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmd0a {0:x2} {1:x2} ", v2, v3));
                            nextoff = off + 2;
                        }
#endif
                        else if (cm8 == 0x40)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            int v4 = br.ReadByte();
                            int v5 = br.ReadByte();
                            dict[off] = new Dis(6, String.Format("Cmd40 {0:x2} {1:x2} {2:x2} {3:x2} {4:x2} ", v0 >> 8, v2, v3, v4, v5));
                            nextoff = off + 3;
                        }
                        else if (cm8 == 0x89)
                        {
                            dict[off] = new Dis(2, String.Format("Ret {0:x2}", v0 >> 8));
                            //nextoff = off + 1;
                            break;
                        }
                        // -- 4 bits cmd
                        else if (cm4 == 0)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            int v4 = br.ReadByte();
                            int v5 = br.ReadByte();
                            dict[off] = new Dis(6, String.Format("Cmd0 {0:x3} {1:x2} {2:x2} {3:x2} {4:x2} ", v0 >> 4, v2, v3, v4, v5));
                            nextoff = off + 3;
                        }
                        else if (cm4 == 1)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmd1 {0:x3} {1:x2} {2:x2} ", v0 >> 4, v2, v3));
                            nextoff = off + 2;
                        }
                        else if (cm4 == 2)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            int v4 = br.ReadByte();
                            int v5 = br.ReadByte();
                            dict[off] = new Dis(6, String.Format("Cmd2 {0:x3} {1:x2} {2:x2} {3:x2} {4:x2} ", v0 >> 4, v2, v3, v4, v5));
                            nextoff = off + 3;
                        }
                        else if (cm4 == 3)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmd3 {0:x3} {1:x2} {2:x2} ", v0 >> 4, v2, v3));
                            nextoff = off + 2;
                        }
                        else if (cm4 == 4)
                        {
                            dict[off] = new Dis(2, String.Format("Cmd4 {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 5)
                        {
                            dict[off] = new Dis(2, String.Format("Cmd5 {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 6)
                        {
                            dict[off] = new Dis(2, String.Format("Cmd6 {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 7)
                        {
                            int v2 = br.ReadInt16();
                            int newoff = off + 2 + v2;
                            dict[off] = new Dis(4, String.Format("J7 {0:x3} {1} ", v0 >> 4, GenLabel(newoff)));
                            offq.Enqueue(newoff);
                            bool noc = (v0 >> 4) == 0;
                            if (noc) break;
                            nextoff = off + 2;
                        }
                        else if (cm4 == 8)
                        {
                            int v2 = br.ReadInt16();
                            int newoff = off + 2 + v2;
                            dict[off] = new Dis(4, String.Format("J8 {0:x3} {1} ", v0 >> 4, GenLabel(newoff)));
                            offq.Enqueue(newoff);
                            nextoff = off + 2;
                        }
                        else if (cm4 == 9)
                        {
                            dict[off] = new Dis(2, String.Format("Pause {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 10)
                        {
                            int v2 = br.ReadByte();
                            int v3 = br.ReadByte();
                            dict[off] = new Dis(4, String.Format("Cmda {0:x3} {1:x2} {2:x2} ", v0 >> 4, v2, v3));
                            nextoff = off + 2;
                        }
                        else if (cm4 == 11)
                        {
                            dict[off] = new Dis(2, String.Format("Cmdb {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 12)
                        {
                            dict[off] = new Dis(2, String.Format("Cmdc {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 13)
                        {
                            dict[off] = new Dis(2, String.Format("Cmdd {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 14)
                        {
                            dict[off] = new Dis(2, String.Format("Cmde {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        else if (cm4 == 15)
                        {
                            dict[off] = new Dis(2, String.Format("Cmdf {0:x3} ", v0 >> 4));
                            nextoff = off + 1;
                        }
                        // -- unknown
                        else
                        {
                            dict[off] = new Dis(1, String.Format("? {0:x4} ", v0, si.Position - 2));
                            break;
                            //nextoff = off + 1;
                            //throw new NotSupportedException(String.Format("{1:X}  {0:x4}", v0, si.Position));
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        errors[off] = null;
                        break;
                    }
                }
            }
        }

        private string GenLabel(int newoff)
        {
            string s;
            if (!labels.TryGetValue(newoff, out s))
            {
                s = String.Format("L{0:0000}", newoff);
                labels[newoff] = s;
            }
            return s;
        }
    }
}
