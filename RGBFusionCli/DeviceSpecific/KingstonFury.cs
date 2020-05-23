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
        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = false)]
        private static extern UIntPtr MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0);
        private static byte pNull = 0;
        private static byte mcuAddr = 78; //Specific for controlling Kingstom RAM from MCU
        private static int commandDelay = 6; 

        private static bool _settingRAMLed = false;

        public static void SetDirect(Color color)
        {
            if (!_settingRAMLed)
            {
                _settingRAMLed = true;
                _ = MCU_Rw(mcuAddr, 225, 1, ref pNull, 0, 0); //Start command
                Thread.Sleep(commandDelay);
                _ = MCU_Rw(mcuAddr, 236, (byte)color.R, ref pNull, 0, 0);
                Thread.Sleep(commandDelay);
                _ = MCU_Rw(mcuAddr, 237, (byte)color.G, ref pNull, 0, 0);
                Thread.Sleep(commandDelay);
                _ = MCU_Rw(mcuAddr, 238, (byte)color.B, ref pNull, 0, 0);
                Thread.Sleep(commandDelay);
                _ = MCU_Rw(mcuAddr, 225, 2, ref pNull, 0, 0); //Close command
                Thread.Sleep(commandDelay);
                _ = MCU_Rw(mcuAddr, 225, 3, ref pNull, 0, 0); // Apply command
                Thread.Sleep(commandDelay);
                _settingRAMLed = false;
            }
        }
    }
}
