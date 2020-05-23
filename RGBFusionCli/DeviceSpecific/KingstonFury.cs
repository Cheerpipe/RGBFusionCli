using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;

namespace RGBFusionCli.Wrappers
{

    public static class KingstonFury
    {
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = false)]
        private static extern UIntPtr MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0);

        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_Skx_SMB_ByteRW", ExactSpelling = true)]
        private static extern uint Skx_SMB_ByteRW(byte controller, byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw);
                                                         //0x27
        private static byte pNull = 0;
        private static byte mcuAddr = 78; //Specific for controlling Kingstom RAM from MCU
        private static int staticAllCommandDelay = 6;

        private static bool _settingRAMLed = false;

        public static void SetIndependentMode()
        {
            _ = MCU_Rw(mcuAddr, 0xe5, 0x21, ref pNull, 0, 0); //Set mode static independent
        }

        public static void SetSingleColorMode()
        {
            _ = MCU_Rw(mcuAddr, 0xe4, 0x09, ref pNull, 0, 0); //Set mode static independent
        }

        private static byte[] _slotBase = new byte[] { 0x11, 0x41, 0x71, 0xA1 };

        public static void SetDimColor(uint slotIndex, Color color)
        {
            if (!_settingRAMLed)
            {
                for (byte led = 0; led < 5; led++)
                {
                    _ = MCU_Rw(mcuAddr, (byte)(_slotBase[slotIndex] + (3 * led) + 0), (byte)color.R, ref pNull, 0, 0);  //set R
                    _ = MCU_Rw(mcuAddr, (byte)(_slotBase[slotIndex] + (3 * led) + 1), (byte)color.G, ref pNull, 0, 0);  //set G
                    _ = MCU_Rw(mcuAddr, (byte)(_slotBase[slotIndex] + (3 * led) + 2), (byte)color.B, ref pNull, 0, 0);  //set B
                    _ = MCU_Rw(mcuAddr, (byte)(_slotBase[slotIndex] + (3 * led) + 16), 0x64, ref pNull, 0, 0); // set Bright max
                }
                _settingRAMLed = false;
            }
        }

        public static void InitLedCommand()
        {
            _ = MCU_Rw(mcuAddr, 0xe1, 0x01, ref pNull, 0, 0); //Start command
        }

        public static void ApplyLedCOmmand()
        {
            _ = MCU_Rw(mcuAddr, 0xe1, 0x02, ref pNull, 0, 0); //Close command
            _ = MCU_Rw(mcuAddr, 0xe1, 0x03, ref pNull, 0, 0); // Apply command
        }

        public static void SetDirect(Color color)
        {
            if (!_settingRAMLed)
            {
                _settingRAMLed = true;
                _ = MCU_Rw(mcuAddr, 0xe1, 0x01, ref pNull, 0, 0); //Start command
                Thread.Sleep(staticAllCommandDelay);
                _ = MCU_Rw(mcuAddr, 0xec, (byte)color.R, ref pNull, 0, 0);
                Thread.Sleep(staticAllCommandDelay);
                _ = MCU_Rw(mcuAddr, 0xed, (byte)color.G, ref pNull, 0, 0);
                Thread.Sleep(staticAllCommandDelay);
                _ = MCU_Rw(mcuAddr, 0xee, (byte)color.B, ref pNull, 0, 0);
                Thread.Sleep(staticAllCommandDelay);
                _ = MCU_Rw(mcuAddr, 0xe1, 0x02, ref pNull, 0, 0); //Close command
                Thread.Sleep(staticAllCommandDelay);
                _ = MCU_Rw(mcuAddr, 0xe1, 0x03, ref pNull, 0, 0); // Apply command
                Thread.Sleep(staticAllCommandDelay);
                _settingRAMLed = false;
            }
        }
    }
}
