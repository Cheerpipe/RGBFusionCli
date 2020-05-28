using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RGBFusionCli
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
    }
}