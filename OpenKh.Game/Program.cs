using McMaster.Extensions.CommandLineUtils;
using Microsoft.Win32.SafeHandles;
using OpenKh.Game.Debugging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Game
{
    [Command("OpenKh.Game")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static readonly string ProductVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [STAThread]
        static void Main(string[] args)
        {
            Log.Info("Boot");
            Log.Info($"Version {ProductVersion}");
            void run()
            {
                Config.Open();
                Config.Listen();

                CommandLineApplication.Execute<Program>(args);

                Config.Close();
                Log.Info("End");
            }

#if DEBUG
            run();
#else
            try
            {
                run();
            }
            catch (Exception ex)
            {
                Log.Err("A fatal error has occurred. Please attach this log to https://github.com/xeeynamo/openkh/issues");
                Catch(ex);
                Log.Close();

                throw ex;
            }
#endif
            Log.Close();
        }

        private static string GetVersion() => ProductVersion;


        [Required]
        [Argument(0, "Content path", "Location of game's data")]
        public string ContentPath { get; }

        [Option(CommandOptionType.NoValue, ShortName = "v", LongName = "console", Description = "Show the console output (Windows only)")]
        public bool ShowConsole { get; set; }

        [Option("--state <ID>", "Boot the game into a specific state (0 = Title, 1 = Map, 2 = Menu)", CommandOptionType.SingleValue)]
        public int InitialState { get; set; }

        [Option("--world <ID>", "Boot the game into a specific world ID (eg. 'dc')", CommandOptionType.SingleValue)]
        public string InitialWorld { get; set; }

        [Option("--place <INDEX>", "Boot the game into a specific place ID (eg. for dc06 specify '6')", CommandOptionType.SingleValue)]
        public int InitialPlace { get; set; }

        [Option("--spawn-map <PROGRAM_ID>", "Force the boot map to use a specific spawn script program ID for MAP", CommandOptionType.SingleValue)]
        public int InitialSpawnScriptMap { get; set; }

        [Option("--spawn-btl <PROGRAM_ID>", "Force the boot map to use a specific spawn script program ID for BTL", CommandOptionType.SingleValue)]
        public int InitialSpawnScriptBtl { get; set; }

        [Option("--spawn-evt <PROGRAM_ID>", "Force the boot map to use a specific spawn script program ID for EVT", CommandOptionType.SingleValue)]
        public int InitialSpawnScriptEvt { get; set; }

        private void OnExecute()
        {
            if (ShowConsole && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const int STD_OUTPUT_HANDLE = -11;
                AllocConsole();
                
                var stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                var safeFileHandle = new SafeFileHandle(stdHandle, true);
                var fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                var standardOutput = new StreamWriter(fileStream, Encoding.UTF8);
                standardOutput.AutoFlush = true;

                Console.SetOut(standardOutput);
            }

            using var game = new OpenKhGame(new OpenKhGameStartup
            {
                ContentPath = ContentPath,
                InitialState = InitialState,
                InitialMap = Kh2.Constants.WorldIds
                    .Select((world, index) => (world, index))
                    .Concat(new (string world, int index)[]
                    {
                        (InitialWorld, -1)
                    })
                    .Where(x => x.world == InitialWorld)
                    .Select(x => x.index)
                    .FirstOrDefault(),
                InitialPlace = InitialPlace,
                InitialSpawnScriptMap = InitialSpawnScriptMap,
                InitialSpawnScriptBtl = InitialSpawnScriptBtl,
                InitialSpawnScriptEvt = InitialSpawnScriptEvt,
            });

            game.Run();
        }

        private static void Catch(Exception ex)
        {
            Log.Err($"{ex.GetType().Name}: {ex.Message}:\n{ex.StackTrace}");
            if (ex.InnerException != null)
                Catch(ex.InnerException);
        }
    }
}