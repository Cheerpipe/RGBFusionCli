using RGBFusionCli;
using RGBFusionCli.Device;
using RGBFusionCli.Device.Aorus2080;
using RGBFusionCli.Device.DledPinHeader;
using RGBFusionCli.Device.KingstonFury;
using RGBFusionCli.Device.RGBFusion;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;

namespace RGBFusionAuroraListener
{
    public static class Program
    {
        internal static RGBFusionLoader _RGBFusionLoader = new RGBFusionLoader();
        internal static Control invokerControl = new Control();
        [STAThread]
        static void Main()
        {
            _RGBFusionLoader.Load();
            DeviceController.Devices.Add(new RGBFusionDevice(_RGBFusionLoader, true));
            DeviceController.Devices.Add(new KingstonFuryDevice());
            DeviceController.Devices.Add(new Aorus2080Device());
            DeviceController.Devices.Add(new Z390DledPinHeaderDevice(_RGBFusionLoader));
            DeviceController.InitAll();
           
            DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(0);
            DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(4);
            DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(5);
            DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(7);
            DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(8);
            DeviceController.GetDeviceByType(DeviceType.RGBFusion).AddLedIndexToIgnoreList(9);
            
            Listener _listener = new Listener();
            _listener.StartArgsPipeServer();

          do { Thread.Sleep(10); } while (true);
        }
    }
}
