using Microsoft.Win32;
using RGBFusionBridge;
using RGBFusionBridge.Device;
using RGBFusionBridge.Device.Aorus2080;
using RGBFusionBridge.Device.DledPinHeader;
using RGBFusionBridge.Device.KingstonFury;
using RGBFusionBridge.Device.RGBFusion;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RGBFusionAuroraListener
{
    internal static class Program
    {
        internal static RGBFusionLoader _RGBFusionLoader = new RGBFusionLoader();
        internal static Listener _listener = new Listener();
        internal static Mutex _singleInstanceMutex = new Mutex(true, "{16eb92ae-ef75-4b7b-a514-7ea1ff92eaf8}");

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains("--ngen"))
            {
                Util.ngen(Util.NgenOperation.Install);
            }
            if (args.Contains("--unngen"))
            {
                Util.ngen(Util.NgenOperation.Uninstall);
            }

            //No more than one instance runing
            if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
                return;

            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.Idle, ThreadPriorityLevel.Lowest);

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            _RGBFusionLoader.Load();
            DeviceController.Devices.Add(new RGBFusionDevice(_RGBFusionLoader, true));

            if (args.Contains("--kingstondriver"))
            {
                DeviceController.Devices.Add(new KingstonFuryDevice());
            }
            if (args.Contains("--aorusvgadriver"))
            {
                DeviceController.Devices.Add(new Aorus2080Device());
            }
            if (args.Contains("--dleddriver"))
            {
                DeviceController.Devices.Add(new Z390DledPinHeaderDevice(_RGBFusionLoader));
            }

            DeviceController.InitAll();


            if (args.Where(arg => arg.Contains("--ignoreled:")).FirstOrDefault()?.Split(':').Length == 2)
            {
                foreach (string lIndex in args.Where(arg => arg.Contains("--ignoreled:")).FirstOrDefault().Split(':')[1].Split(','))
                {
                    DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(Convert.ToByte(lIndex));
                }
            }

            _listener.Start();
            Application.Run();
        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            _listener.Stop();
            DeviceController.Shutdown();
            Thread.Sleep(1000);
            Application.Exit();
        }
    }
}
