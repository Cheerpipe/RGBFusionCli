using Microsoft.Win32;
using RGBFusionCli;
using RGBFusionCli.Device;
using RGBFusionCli.Device.Aorus2080;
using RGBFusionCli.Device.DledPinHeader;
using RGBFusionCli.Device.KingstonFury;
using RGBFusionCli.Device.RGBFusion;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

namespace RGBFusionAuroraListener
{
    public static class Program
    {
        internal static RGBFusionLoader _RGBFusionLoader = new RGBFusionLoader();
        internal static Control invokerControl = new Control();
        internal static Listener _listener = new Listener();
        static Mutex mutex = new Mutex(true, "{16eb92ae-ef75-4b7b-a514-7ea1ff92eaf8}");

        [STAThread]
        static void Main(string[] args)
        {
            //No more than one instance runing

            if (!mutex.WaitOne(TimeSpan.Zero, true))
                return;

            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.Idle, ThreadPriorityLevel.Lowest);

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            _RGBFusionLoader.Load();
            DeviceController.Devices.Add(new RGBFusionDevice(_RGBFusionLoader, true));
            //DeviceController.Devices.Add(new KingstonFuryDevice());
            //DeviceController.Devices.Add(new Aorus2080Device());
            //DeviceController.Devices.Add(new Z390DledPinHeaderDevice(_RGBFusionLoader));

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
            do { Thread.Sleep(10); } while (_listener.Listening);
            Thread.Sleep(1000);
        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            DeviceController.Shutdown();
        }
    }
}
