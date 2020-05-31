using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusionBridge.Device
{
    public interface IDevice
    {
        void Init();
        void SetLed(Color color, byte ledIndex);
        HashSet<byte> GetAreaIndexes();
        void Apply();
        void Cancel();
        bool LedDataChanged();
        DeviceType GetDeviceType();
        bool AddLedIndexToIgnoreList(byte ledIndex);
        bool RemoveLedIndexToIgnoreList(byte ledIndex);
        void Shutdown();
    }
}
