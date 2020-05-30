using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace RGBFusionBridge.Device.KingstonFury
{
    public class KingstonFuryDevice : Device
    {
        [SuppressUnmanagedCodeSecurity()]
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = true)]
        private static extern uint MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0U);
        private byte _pNull = 0;
        private byte _mcuAddr = 78; //Specific for controlling Kingstom RAM from MCU
        private int _commandDelay = 1;
        private bool _changingColor = false;

        public override void Init()
        {
            _deviceType = DeviceType.KingstonFury;
            _ledIndexes.Add(0);
            SetStaticMode();
            SetBright(255);
            base.Init();
        }

        public void SetStaticMode()
        {
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0u); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe4, 0x09, ref _pNull, 0, 0u); //Set mode static all
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0u); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0u); // Apply command
            Thread.Sleep(_commandDelay);
        }

        public void SetBright(byte bright)
        {
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0u); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xdd, 0x64, ref _pNull, 0, 0u); // Brightness
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0u); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0u); // Apply command
            Thread.Sleep(_commandDelay);
        }

        public override void Apply()
        {
            if (!_changingColor)
            {
                SendColorToRAM();
                base.Apply();
                _changingColor = false;
            }
        }


        private void SendColorToRAM()
        {
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0u); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xec, _newLedData[0], ref _pNull, 0, 0u); // R Register
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xed, _newLedData[1], ref _pNull, 0, 0u); // G Register
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xee, _newLedData[2], ref _pNull, 0, 0u); // B Register
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xdd, 0x64, ref _pNull, 0, 0u); // Brightness
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0u); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0u); // Apply command
            Thread.Sleep(_commandDelay);
        }

        public override void Shutdown()
        {
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }

        protected override void ConfirmApply()
        {
            SendColorToRAM();
        }
    }
}
