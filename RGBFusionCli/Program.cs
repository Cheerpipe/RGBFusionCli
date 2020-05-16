using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;


namespace RGBFusionCli
{
    internal static class Program
    {
        private static RgbFusion _controller;
        private static Transaction _transaction;

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

            _controller = new RgbFusion();
            _transaction = new Transaction(_controller);
            Util.MinimizeMemory();
            pipeInterOp.StartArgsPipeServer();
            Util.MinimizeMemory();
            _controller.Init(false);
            pipeInterOp.SendArgs(args);
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            _controller.StartListening();

        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            Run(new string[] { "--shutdown" });
            Thread.Sleep(500);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var otherCompanyDlls = new DirectoryInfo(@"C:\Program Files (x86)\GIGABYTE\RGBFusion\").GetFiles("*.dll");
            var dll = otherCompanyDlls.FirstOrDefault(fi => fi.Name == args.Name);
            if (dll == null)
            {
                return null;
            }

            return Assembly.Load(dll.FullName);

        }

        public static void Run(string[] args)
        {
            if (_controller?.IsInitialized() == false)
                return;

            var _ledCommands = CommandLineParser.GetLedCommands(args);
            if (_ledCommands.Count > 0)
            {
                if (_transaction.TransactioStarted)
                {
                    _transaction.TransactionSetZoned(_ledCommands);
                    return;
                }
                _controller?.ChangeColorForAreas(_ledCommands);
                return;
            }

            int traxMaxAlive = CommandLineParser.GetTransactionStartCommand(args);
            if (traxMaxAlive > -1)
            {
                _transaction.TransactionStart(traxMaxAlive);
                return;
            }

            if (CommandLineParser.GetTransactionCommitCommand(args))
            {
                try

                {
                    _transaction.TransactionCommit();
                }
                catch //No transaction started
                { }
            }

            if (CommandLineParser.GetTransactionCancel(args))
            {
                _transaction.TransactionCancel();
                return;
            }

            if (CommandLineParser.GeShutdownCommand(args))
            {
                //Shutdown this shit
                Run(new string[] { "--sa:-1:0:0:0:0" });
                Thread.Sleep(500);
                _controller.Shutdown();
                Thread.Sleep(500);
            }

            int _profileCommandIndex = CommandLineParser.LoadProfileCommand(args);
            if (_profileCommandIndex > 0)
            {
                _controller?.LoadProfile(_profileCommandIndex);
            }
            else if (CommandLineParser.GetAreasCommand(args))
                MessageBox.Show(_controller?.GetAreasReport());
        }
    }
}
