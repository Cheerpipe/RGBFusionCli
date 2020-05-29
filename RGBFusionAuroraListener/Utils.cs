using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RGBFusionAuroraListener
{
    public class Util
    {
        [DllImport(dllName: "kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr process, UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);
        public static void MinimizeMemory()
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)0xFFFFFFFF, (UIntPtr)0xFFFFFFFF);
        }

        public static void SetPriorityProcessAndThreads(string nameProcess, ProcessPriorityClass processPriority, ThreadPriorityLevel threadPriorityLevel)
        {
            foreach (var a in Process.GetProcessesByName(nameProcess))
            {
                a.PriorityBoostEnabled = true;
                a.PriorityClass = processPriority;

                foreach (ProcessThread processThread in a.Threads)
                {
                    processThread.PriorityLevel = threadPriorityLevel;
                    processThread.PriorityBoostEnabled = true;
                }
            }
        }

		public static void ngen(Util.NgenOperation ngenOperation)
		{
			string fileName = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
			string executablePath = Application.ExecutablePath;
			string arg = (ngenOperation == Util.NgenOperation.Install) ? "install" : "uninstall";
			string name = Assembly.GetExecutingAssembly().GetName().Name;
			Process process = new Process();
			process.StartInfo.FileName = fileName;
			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.Arguments = string.Format("{0} {1}", arg, executablePath);
			process.Start();
			process.WaitForExit();
			bool flag = process.ExitCode == 0;
			if (flag)
			{
				MessageBox.Show(string.Format("NGEN operaction {0} on assembly {1} executed successfully.", arg, name));
			}
			else
			{
				MessageBox.Show(string.Format("NGEN operaction {0} on assembly {1} failed with code {2}.", arg, name, process.ExitCode));
			}
		}

		public enum NgenOperation
		{
			Install,
			Uninstall
		}
	}
}