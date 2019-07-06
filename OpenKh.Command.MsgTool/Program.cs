using OpenKh.Common;
using OpenKh.Kh2.Messages;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Xml.Linq;
using System.Text;

namespace OpenKh.Command.MsgTool
{
    class Program
    {
        static string filename = @"D:\Hacking\KH2\reseach\msn\EH21_MS101\ms_b_0.ai";

        static void Main(string[] args)
        {
            File.OpenRead(filename).Using(x =>
            {
                var asd = new ParseAI.Parse03(Console.Out);
                asd.Run(x.ReadAllBytes());
            });
        }
    }
}
