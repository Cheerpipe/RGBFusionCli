using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusionBridge.Device.KingstonFury
{
    public class KingstonRamStick
    {
        public readonly byte StickPosition;
        public readonly KingstonRamLed[] Leds;
        public KingstonRamStick(byte stickPosition)
        {
            StickPosition = stickPosition;
            Leds = new KingstonRamLed[5];
            for (byte l = 0; l < 5; l++)
            {
                Leds[l] = new KingstonRamLed(l);
            }
        }
    }

    public class KingstonRamLed
    {
        byte LedPosition;
        public byte RedRegister;
        public byte GreenRegister;
        public byte BlueRegister;
        public byte BrightRegister;
        public KingstonRamLed(byte ledPosition)
        {
            LedPosition = ledPosition;
        }
    }

    public class KingstonRamRegisterMap
    {
        public readonly KingstonRamStick[] Sticks = new KingstonRamStick[4];
        public KingstonRamRegisterMap()
        {
            Sticks[0] = new KingstonRamStick(0);
            Sticks[1] = new KingstonRamStick(1);
            Sticks[2] = new KingstonRamStick(2);
            Sticks[3] = new KingstonRamStick(3);

            Sticks[0].Leds[0].RedRegister = 0x11; // Slot 0 LED 0 Red
            Sticks[0].Leds[0].GreenRegister = 0x12; // Slot 0 LED 0 Green
            Sticks[0].Leds[0].BlueRegister = 0x13; // Slot 0 LED 0 Blue
            Sticks[0].Leds[1].RedRegister = 0x14; // Slot 0 LED 1 Red
            Sticks[0].Leds[1].GreenRegister = 0x15; // Slot 0 LED 1 Green
            Sticks[0].Leds[1].BlueRegister = 0x16; // Slot 0 LED 1 Blue
            Sticks[0].Leds[2].RedRegister = 0x17; // Slot 0 LED 2 Red
            Sticks[0].Leds[2].GreenRegister = 0x18; // Slot 0 LED 2 Green
            Sticks[0].Leds[2].BlueRegister = 0x19; // Slot 0 LED 2 Blue
            Sticks[0].Leds[3].RedRegister = 0x1A; // Slot 0 LED 3 Red
            Sticks[0].Leds[3].GreenRegister = 0x1B; // Slot 0 LED 3 Green
            Sticks[0].Leds[3].BlueRegister = 0x1C; // Slot 0 LED 3 Blue
            Sticks[0].Leds[4].RedRegister = 0x1D; // Slot 0 LED 4 Red
            Sticks[0].Leds[4].GreenRegister = 0x1E; // Slot 0 LED 4 Green
            Sticks[0].Leds[4].BlueRegister = 0x1F; // Slot 0 LED 4 Blue
            Sticks[0].Leds[0].BrightRegister = 0x21; // Slot 0 LED 0 Brightness (0-100)
            Sticks[0].Leds[1].BrightRegister = 0x24; // Slot 0 LED 1 Brightness (0-100)
            Sticks[0].Leds[2].BrightRegister = 0x27; // Slot 0 LED 2 Brightness (0-100)
            Sticks[0].Leds[3].BrightRegister = 0x2A; // Slot 0 LED 3 Brightness (0-100)
            Sticks[0].Leds[4].BrightRegister = 0x2D; // Slot 0 LED 4 Brightness (0-100)

            Sticks[1].Leds[0].RedRegister = 0x41; // Slot 1 LED 0 Red
            Sticks[1].Leds[0].GreenRegister = 0x42; // Slot 1 LED 0 Green
            Sticks[1].Leds[0].BlueRegister = 0x43; // Slot 1 LED 0 Blue
            Sticks[1].Leds[1].RedRegister = 0x44; // Slot 1 LED 1 Red
            Sticks[1].Leds[1].GreenRegister = 0x45; // Slot 1 LED 1 Green
            Sticks[1].Leds[1].BlueRegister = 0x46; // Slot 1 LED 1 Blue
            Sticks[1].Leds[2].RedRegister = 0x47; // Slot 1 LED 2 Red
            Sticks[1].Leds[2].GreenRegister = 0x48; // Slot 1 LED 2 Green
            Sticks[1].Leds[2].BlueRegister = 0x49; // Slot 1 LED 2 Blue
            Sticks[1].Leds[3].RedRegister = 0x4A; // Slot 1 LED 3 Red
            Sticks[1].Leds[3].GreenRegister = 0x4B; // Slot 1 LED 3 Green
            Sticks[1].Leds[3].BlueRegister = 0x4C; // Slot 1 LED 3 Blue
            Sticks[1].Leds[4].RedRegister = 0x4D; // Slot 1 LED 4 Red
            Sticks[1].Leds[4].GreenRegister = 0x4E; // Slot 1 LED 4 Green
            Sticks[1].Leds[4].BlueRegister = 0x4F; // Slot 1 LED 4 Blue
            Sticks[1].Leds[0].BrightRegister = 0x51; // Slot 1 LED 0 Brightness (0-100)
            Sticks[1].Leds[1].BrightRegister = 0x54; // Slot 1 LED 1 Brightness (0-100)
            Sticks[1].Leds[2].BrightRegister = 0x57; // Slot 1 LED 2 Brightness (0-100)
            Sticks[1].Leds[3].BrightRegister = 0x5A; // Slot 1 LED 3 Brightness (0-100)
            Sticks[1].Leds[4].BrightRegister = 0x5D; // Slot 1 LED 4 Brightness (0-100)

            Sticks[2].Leds[0].RedRegister = 0x71; // Slot 2 LED 0 Red
            Sticks[2].Leds[0].GreenRegister = 0x72; // Slot 2 LED 0 Green
            Sticks[2].Leds[0].BlueRegister = 0x73; // Slot 2 LED 0 Blue
            Sticks[2].Leds[1].RedRegister = 0x74; // Slot 2 LED 1 Red
            Sticks[2].Leds[1].GreenRegister = 0x75; // Slot 2 LED 1 Green
            Sticks[2].Leds[1].BlueRegister = 0x76; // Slot 2 LED 1 Blue
            Sticks[2].Leds[2].RedRegister = 0x77; // Slot 2 LED 2 Red
            Sticks[2].Leds[2].GreenRegister = 0x78; // Slot 2 LED 2 Green
            Sticks[2].Leds[2].BlueRegister = 0x79; // Slot 2 LED 2 Blue
            Sticks[2].Leds[3].RedRegister = 0x7A; // Slot 2 LED 3 Red
            Sticks[2].Leds[3].GreenRegister = 0x7B; // Slot 2 LED 3 Green
            Sticks[2].Leds[3].BlueRegister = 0x7C; // Slot 2 LED 3 Blue
            Sticks[2].Leds[4].RedRegister = 0x7D; // Slot 2 LED 4 Red
            Sticks[2].Leds[4].GreenRegister = 0x7E; // Slot 2 LED 4 Green
            Sticks[2].Leds[4].BlueRegister = 0x7F; // Slot 2 LED 4 Blue
            Sticks[2].Leds[0].BrightRegister = 0x81; // Slot 2 LED 0 Brightness (0-100)
            Sticks[2].Leds[1].BrightRegister = 0x84; // Slot 2 LED 1 Brightness (0-100)
            Sticks[2].Leds[2].BrightRegister = 0x87; // Slot 2 LED 2 Brightness (0-100)
            Sticks[2].Leds[3].BrightRegister = 0x8A; // Slot 2 LED 3 Brightness (0-100)
            Sticks[2].Leds[4].BrightRegister = 0x8D; // Slot 2 LED 4 Brightness (0-100)

            Sticks[3].Leds[0].RedRegister = 0xA1; // Slot 3 LED 0 Red
            Sticks[3].Leds[0].GreenRegister = 0xA2; // Slot 3 LED 0 Green
            Sticks[3].Leds[0].BlueRegister = 0xA3; // Slot 3 LED 0 Blue
            Sticks[3].Leds[1].RedRegister = 0xA4; // Slot 3 LED 1 Red
            Sticks[3].Leds[1].GreenRegister = 0xA5; // Slot 3 LED 1 Green
            Sticks[3].Leds[1].BlueRegister = 0xA6; // Slot 3 LED 1 Blue
            Sticks[3].Leds[2].RedRegister = 0xA7; // Slot 3 LED 2 Red
            Sticks[3].Leds[2].GreenRegister = 0xA8; // Slot 3 LED 2 Green
            Sticks[3].Leds[2].BlueRegister = 0xA9; // Slot 3 LED 2 Blue
            Sticks[3].Leds[3].RedRegister = 0xAA; // Slot 3 LED 3 Red
            Sticks[3].Leds[3].GreenRegister = 0xAB; // Slot 3 LED 3 Green
            Sticks[3].Leds[3].BlueRegister = 0xAC; // Slot 3 LED 3 Blue
            Sticks[3].Leds[4].RedRegister = 0xAD; // Slot 3 LED 4 Red
            Sticks[3].Leds[4].GreenRegister = 0xAE; // Slot 3 LED 4 Green
            Sticks[3].Leds[4].BlueRegister = 0xAF; // Slot 3 LED 4 Blue
            Sticks[3].Leds[0].BrightRegister = 0xB1; // Slot 3 LED 0 Brightness (0-100)
            Sticks[3].Leds[1].BrightRegister = 0xB4; // Slot 3 LED 1 Brightness (0-100)
            Sticks[3].Leds[2].BrightRegister = 0xB7; // Slot 3 LED 2 Brightness (0-100)
            Sticks[3].Leds[3].BrightRegister = 0xBA; // Slot 3 LED 3 Brightness (0-100)
            Sticks[3].Leds[4].BrightRegister = 0xBD; // Slot 3 LED 4 Brightness (0-100)
        }
    }
}