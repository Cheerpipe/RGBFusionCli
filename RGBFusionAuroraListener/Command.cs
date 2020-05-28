using RGBFusionCli.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusionAuroraListener
{

    /* Command ID: 
            1: SetLed
            2: Apply
            3: Add led to ignore list
            4: Remove led from ignore list
      
      Device Type
            0       :   All (Call Controller Methods)
            100     :   RGBFusion

            Use below devices only if you know what are you doing or use just
            200     :   DledPinHeader 
            300     :   KingstonFury
            400     :   Aorus2080
     */
    public class Command
    {
        byte[] _commandBytes;
        public byte CommandId { get; }
        public DeviceType DeviceType { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte LedIndex { get; }

        public Command(byte[] commandBytes)
        {
            _commandBytes = commandBytes;
            CommandId = commandBytes[0];
            DeviceType = (DeviceType)commandBytes[1];
            R = commandBytes[2];
            G = commandBytes[3];
            B = commandBytes[4];
            LedIndex = commandBytes[5];
        }

        public Command(byte commandId, DeviceType deviceType, byte r, byte g, byte b, byte ledIndex)
        {
            CommandId = commandId;
            DeviceType = deviceType;
            R = r;
            G = g;
            B = b;
            LedIndex = ledIndex;

            _commandBytes[0] = CommandId;
            _commandBytes[1] = (byte)DeviceType;
            _commandBytes[2] = R;
            _commandBytes[3] = G;
            _commandBytes[4] = B;
            _commandBytes[5] = LedIndex;
        }

        public byte[] GetBytes()
        {
            return _commandBytes;
        }
    }
}
