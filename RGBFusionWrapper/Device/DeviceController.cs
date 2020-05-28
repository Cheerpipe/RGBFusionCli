using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RGBFusionCli.Device
{
    public static class DeviceController
    {
        private static List<IDevice> _devices = new List<IDevice>();
        public static List<IDevice> Devices { get => _devices; set => _devices = value; }

        public static IDevice GetDeviceByType(DeviceType deviceType)
        {
            return _devices.Where(d => d.GetDeviceType() == deviceType).FirstOrDefault();
        }

        public static void ApplyAll()
        {
            foreach (IDevice d in _devices)
            {
                if (d.LedDataChanged())
                {
                    new Task(() => { d.Apply(); }).Start();
                }
            }
        }

        public static void InitAll()
        {
            foreach (IDevice d in _devices)
            {
                d.Init();
            }
        }
    }
}
