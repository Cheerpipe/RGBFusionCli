using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace RGBFusionCli.Device.KingstonFury
{
    public class KingstonFuryDevice : Device
    {
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = true)]
        private static extern UIntPtr MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0);
        private byte _pNull = 0;
        private byte _mcuAddr = 78; //Specific for controlling Kingstom RAM from MCU
        private int _commandDelay = 1;
        private bool _changingColor = false;

        public override void Init()
        {
            _deviceType = DeviceType.KingstonFury;
            _ledIndexes.Add(0);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe4, 0x09, ref _pNull, 0, 0); //Set mode static independent
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0); // Apply command
            Thread.Sleep(_commandDelay);
            base.Init();
        }

        public override void Apply()
        {
            if (!_changingColor)
            {
                _changingColor = true;
                _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0); //Start command
                Thread.Sleep(_commandDelay);
                _ = MCU_Rw(_mcuAddr, 0xec, _newLedData[0], ref _pNull, 0, 0); // R Register
                Thread.Sleep(_commandDelay);
                _ = MCU_Rw(_mcuAddr, 0xed, _newLedData[1], ref _pNull, 0, 0); // G Register
                Thread.Sleep(_commandDelay);
                _ = MCU_Rw(_mcuAddr, 0xee, _newLedData[2], ref _pNull, 0, 0); // B Register
                Thread.Sleep(_commandDelay);
                _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0); //Close command
                Thread.Sleep(_commandDelay);
                _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0); // Apply command
                Thread.Sleep(_commandDelay);
                base.Apply();
                _changingColor = false;
            }
        }
        public override void Shutdown()
        {
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }

    }
}
