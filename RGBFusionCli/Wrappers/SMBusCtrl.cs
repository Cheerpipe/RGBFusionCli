using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RGBFusionCli.Wrappers
{

    public static class SMBusCtrl
    {

        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_MCU_Rw", ExactSpelling = true)]
        public static extern uint MCU_Rw(byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw, uint delayTime = 0);

        [DllImport("SMBCtrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.None, EntryPoint = "dllexp_Skx_SMB_ByteRW", ExactSpelling = true)]
        public static extern uint Skx_SMB_ByteRW(byte controller, byte mcuAddr, byte regOffset, byte val, ref byte pVal, byte rw);

    }
}
