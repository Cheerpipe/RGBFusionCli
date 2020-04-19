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

            var _ledCommands = CommandLineParser.GetLedCommands(args);
            if (_ledCommands.Count > 0)
            {
                _fusion?.ChangeColorForAreas(_ledCommands);
                return;
            }
            if (CommandLineParser.GetResetCommand(args))
            {
                _fusion?.Reset();
                return;
            }
            int _profileCommandIndex = CommandLineParser.LoadProfileCommand(args);
            if (_profileCommandIndex > 0)
            {
                _fusion?.LoadProfile(_profileCommandIndex);
            }
            else if (CommandLineParser.GetAreasCommand(args))
                MessageBox.Show(_fusion?.GetAreasReport());
        }
    }
}
