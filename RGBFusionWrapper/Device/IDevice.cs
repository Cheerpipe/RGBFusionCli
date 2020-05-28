using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusionCli.Device
{
    public interface IDevice
    {
        void Init();
        void SetLed(Color color, byte ledIndex);
        HashSet<int> GetAreaIndexes();
        void Apply();
        void Cancel();
        bool LedDataChanged();
        DeviceType GetDeviceType();
        bool AddLedIndexToIgnoreList(int ledIndex);
        bool RemoveLedIndexToIgnoreList(int ledIndex);
    }
}
