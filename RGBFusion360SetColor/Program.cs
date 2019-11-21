using System;
using System.Diagnostics;
using System.Windows;


namespace RGBFusion390SetColor
{
    internal static class Program
    {
        private static RgbFusion _fusion;
        [STAThread]
        private static void Main(string[] args)
        {
            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.Idle, ThreadPriorityLevel.Lowest);

            var pipeInterOp = new ArgsPipeInterOp();
            var thisProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
            if (thisProcess > 1)
            {
                pipeInterOp.SendArgs(args);
                return;
            }

            _fusion = new RgbFusion();
            Util.MinimizeMemory();
            pipeInterOp.StartArgsPipeServer();
            Util.MinimizeMemory();
            _fusion.Init();
        }

        public static void Run(string[] args)
        {
            if (_fusion?.IsInitialized() == false)
                return;

            if (CommandLineParser.LoadProfileCommand(args) > 0)
            {
                _fusion?.LoadProfile(CommandLineParser.LoadProfileCommand(args));
            }
            if (CommandLineParser.GetLedCommands(args).Count > 0)
            {
                _fusion?.ChangeColorForAreas(CommandLineParser.GetLedCommands(args));
            }
            else if (CommandLineParser.GetAreasCommand(args))
                MessageBox.Show(_fusion?.GetAreasReport());
            else if (CommandLineParser.StartMusicMode(args))
                _fusion?.StartMusicMode();
            else if (CommandLineParser.StopMusicMode(args))
                _fusion?.StopMusicMode();
        }
    }
}
