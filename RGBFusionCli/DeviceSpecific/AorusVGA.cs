using Gigabyte.ULightingEffects.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RGBFusionCli
{
    //For single LED nvidia VGAs
    static public class AorusVGA
    {
        [DllImport("GvLedLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_GvLedSet", ExactSpelling = true)]
        private static extern uint GvLedSet(int nIndex, GVLED_CFG_V1 config);
        [DllImport("GvLedLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_GvLedSave", ExactSpelling = true)]
        private static extern uint GvLedSave(int nIndex, GVLED_CFG_V1 config);
        private static GVLED_CFG_V1 curSetting = new GVLED_CFG_V1(1, 0, 0, 0, 10, 16711680);

        private static bool _settingVGALed = false;
        public static void SetDirect(Color color)
        {
            if (!_settingVGALed)
            {
                _settingVGALed = true;
                int _VGARGBNewColor = ((color.R & 0x0ff) << 16) | ((color.G & 0x0ff) << 8) | (color.B & 0x0ff);
                curSetting.dwColor = (uint)_VGARGBNewColor & 16777215;
                curSetting.nSync = 1;
                GvLedSet(4097, curSetting);
                Thread.Sleep(5);
                _settingVGALed = false;
            }
        }
    }
}
