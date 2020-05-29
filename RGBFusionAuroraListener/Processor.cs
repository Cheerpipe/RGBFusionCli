using RGBFusionBridge.Device;
using System;
using System.Drawing;
using System.Threading;

namespace RGBFusionAuroraListener
{
    public static class Processor
    {
        public static void ProcessCommand(byte[] commandBytes)
        {
            Command command = new Command(commandBytes);

            switch (command.CommandId)
            {
                case 1://Setled
                    DeviceController.GetDeviceByType(command.DeviceType).SetLed(Color.FromArgb(command.R, command.G, command.B), command.LedIndex);
                    break;
                case 2://Apply
                    if (command.DeviceType == DeviceType.Unknown)
                        DeviceController.ApplyAll();
                    else
                        DeviceController.GetDeviceByType(command.DeviceType).Apply();
                    break;
                case 3://Add led to ignore list
                    DeviceController.GetDeviceByType(command.DeviceType).AddLedIndexToIgnoreList(command.LedIndex);
                    break;

                case 4://Remove led to ignore list
                    DeviceController.GetDeviceByType(command.DeviceType).RemoveLedIndexToIgnoreList(command.LedIndex);
                    break;

                case 5://Shutdown
                    Program._listener.Stop();
                    if (command.DeviceType == DeviceType.Unknown)
                        DeviceController.Shutdown();
                    else
                        DeviceController.GetDeviceByType(command.DeviceType).Shutdown();
                    Thread.Sleep(1000);
                    System.Windows.Forms.Application.Exit();
                    break;
            }
        }
    }
}