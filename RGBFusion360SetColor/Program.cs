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
            var pipeInterOp = new ArgsPipeInterOp();
            var instanceCount = int.MaxValue;
            if (!CommandLineParser.NoInstanceCheck(args))
            {
                instanceCount = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
            }

            if (instanceCount > 1)
            {
                pipeInterOp.SendArgs(args);
                return;
            }

            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.Idle, ThreadPriorityLevel.Lowest);

            _fusion = new RgbFusion();
            Util.MinimizeMemory();
            pipeInterOp.StartArgsPipeServer();
            Util.MinimizeMemory();
            _fusion.Init(false);
            pipeInterOp.SendArgs(args);
            _fusion.StartListening();

        }

        public static void Run(string[] args)
        {
            if (_fusion?.IsInitialized() == false)
                return;

            if (CommandLineParser.GetLedCommands(args).Count > 0)
            {
                _fusion?.ChangeColorForAreas(CommandLineParser.GetLedCommands(args));
                return;
            }
            if (CommandLineParser.GetResetCommand(args))
            {
                _fusion?.Reset();
                return;
            }
            if (CommandLineParser.LoadProfileCommand(args) > 0)
            {
                _fusion?.LoadProfile(CommandLineParser.LoadProfileCommand(args));
            }
            else if (CommandLineParser.GetAreasCommand(args))
                MessageBox.Show(_fusion?.GetAreasReport());
        }
    }
}
