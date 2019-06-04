using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common.Archives;
using System;
using System.IO;

namespace OpenKh.Command.PsarcTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        [Argument(0, "PSARC file")]
        private string PsarcFile { get; }

        [Option(ShortName = "p", LongName = "print-info", Description = "Displays informations about content of file.")]
        private bool PrintInfo { get; }

        [Option(ShortName = "x", LongName = "extract", Description = "Extracts the file.")]
        private bool ExtractFile { get; }

        private void OnExecute()
        {
            Psarc psarc = new Psarc(File.OpenRead(PsarcFile));
            if (PrintInfo)
            {
                Console.WriteLine($"TOC Entries: {psarc.TocEntries}");
                foreach (var tocEntry in psarc.Toc)
                    Console.WriteLine(tocEntry.ToString());
            }
            else if (ExtractFile)
            {
                var rootDirName = Path.GetFileNameWithoutExtension(PsarcFile);
                Directory.CreateDirectory(rootDirName);
                foreach (var tocEntry in psarc.Toc)
                {
                    var outFileName = Path.Combine(rootDirName, tocEntry.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(outFileName));

                    File.WriteAllBytes(outFileName, psarc.DecompressFile((int)tocEntry.Index));
                    Console.WriteLine($"Extracting {tocEntry.FileName}");
                }
            }
        }
    }
}
