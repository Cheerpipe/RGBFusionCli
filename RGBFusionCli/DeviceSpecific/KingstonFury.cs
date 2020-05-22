using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RGBFusionCli.Wrappers
{

    public static class KingstonFury
    {
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = true)]
        private static extern uint MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0);
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_Skx_SMB_ByteRW", ExactSpelling = true)]
        private static extern uint Skx_SMB_ByteRW(byte controller, byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw);
        private static byte pNull = 0;
        private static byte mcuAddr = 78; //Specific for controlling Kingstom RAM from MCU
        private static int commandDelay = 1; // It could be too lower if ther are more devices using MCU. Setting to 1 because i have only rams using mcu.

        private static bool _settingRAMLed = false;
        public static void SetDirect(Color color)
        {
            if (!_settingRAMLed)
            {
                _settingRAMLed = true;
                MCU_Rw(mcuAddr, 225, 1, ref pNull, 0, 0); //Start command
                MCU_Rw(mcuAddr, 236, (byte)color.R, ref pNull, 0, 0);
                Thread.Sleep(commandDelay);
                MCU_Rw(mcuAddr, 237, (byte)color.G, ref pNull, 0, 0);
                Thread.Sleep(commandDelay);
                MCU_Rw(mcuAddr, 238, (byte)color.B, ref pNull, 0, 0);
                Thread.Sleep(commandDelay);
                MCU_Rw(mcuAddr, 225, 2, ref pNull, 0, 0); //Close command
                MCU_Rw(mcuAddr, 225, 3, ref pNull, 0, 0); // Apply command
                _settingRAMLed = false;
            }
        }

    }
}
