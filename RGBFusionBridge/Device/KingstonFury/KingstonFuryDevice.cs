using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace RGBFusionBridge.Device.KingstonFury
{
    enum CurrentRamMode
    {
        Unknown,
        Static,
        IndividuallyAddressableStatic
    }

    public class KingstonFuryDevice : Device
    {
        [SuppressUnmanagedCodeSecurity()]
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = true)]
        private static extern uint MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0U);
        private byte _pNull = 0;
        private byte _mcuAddr = 78; //Specific for controlling Kingstom RAM from MCU
        private int _commandDelay = 5;
        private bool _changingColor = false;
        private CurrentRamMode _currentramMode = CurrentRamMode.Unknown;
        private KingstonRamRegisterMap _kingstonRamRegisterMap = new KingstonRamRegisterMap();
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
            if (_currentramMode == CurrentRamMode.Static)
                return;
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0u); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe4, 0x09, ref _pNull, 0, 0u); //Set mode static all
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0u); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0u); // Apply command
            Thread.Sleep(_commandDelay);
            _currentramMode = CurrentRamMode.Static;
        }

        public void SetIndividuallyAddressableStatic()
        {
            if (_currentramMode == CurrentRamMode.IndividuallyAddressableStatic)
                return;
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0u); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe5, 0x21, ref _pNull, 0, 0u); //Set mode static all
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0u); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0u); // Apply command
            Thread.Sleep(_commandDelay);
            _currentramMode = CurrentRamMode.IndividuallyAddressableStatic;
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
                _changingColor = true;
                SendColorToRAM();
                base.Apply();
                _changingColor = false;
            }
        }

        private void SendColorToLed(byte stick, byte led, Color color)
        {
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x01, ref _pNull, 0, 0u); //Start command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, _kingstonRamRegisterMap.Sticks[stick].Leds[led].RedRegister, _newLedData[0], ref _pNull, 0, 0u); // R Register
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, _kingstonRamRegisterMap.Sticks[stick].Leds[led].GreenRegister, _newLedData[1], ref _pNull, 0, 0u); // G Register
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, _kingstonRamRegisterMap.Sticks[stick].Leds[led].BlueRegister, _newLedData[2], ref _pNull, 0, 0u); // B Register
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x02, ref _pNull, 0, 0u); //Close command
            Thread.Sleep(_commandDelay);
            _ = MCU_Rw(_mcuAddr, 0xe1, 0x03, ref _pNull, 0, 0u); // Apply command
            Thread.Sleep(_commandDelay);
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
