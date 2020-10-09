using LedLib2;
using SelLEDControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using LedLib2.IT8297;
using System.Drawing;
using System.Linq;

namespace RGBFusionBridge.Device.DledPinHeader
{
    public class Z390DledPinHeaderDevice : Device
    {
        //Div 5 / strip 0 = bottom DLED
        //Div 6 / strip 1 = top DLED
        private RGBFusionLoader _RGBFusionController;
        private LedObject _ledObject;
        private MethodInfo _outputLED0;
        private MethodInfo _outputLED1;
        private int _times0;
        private int _times1;
        private int _nData0;
        private int _nData1;
        private int _iLed0;
        private int _iLed1;
        private object _coll97;
        private List<MCU_8297> _lstM97;

        public Z390DledPinHeaderDevice(RGBFusionLoader rgbFusionController)
        {
            _RGBFusionController = rgbFusionController;
            _ledObject = (LedObject)typeof(Comm_LED_Fun).GetField("LedObj", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_RGBFusionController._ledFun);
            _coll97 = typeof(LedObject).GetField("coll97", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_ledObject);
            _lstM97 = (List<MCU_8297>)typeof(LedObject).Assembly.CreateInstance("LedLib2.IT8297.Collection_8297").GetType().GetField("lstM97", BindingFlags.Public | BindingFlags.Instance).GetValue(_coll97);
            _outputLED0 = _lstM97[0].GetType().GetMethod("DStrip0_Out_LED", BindingFlags.NonPublic | BindingFlags.Instance);
            _outputLED1 = _lstM97[0].GetType().GetMethod("DStrip1_Out_LED", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void Init()
        {
            _deviceType = DeviceType.DledPinHeader;
            for (byte l = 0; l < 64; l++)
                _ledIndexes.Add(l);
            base.Init();
            ResizeCtrlLen(_ledIndexes.Count / 2); //requerido
            _lstM97[0].Enable_DLedStripCtrl(0, true);
            Apply();
            _lstM97[0].Enable_DLedStripCtrl(1, true);
            Apply();


        }

        public override void SetLed(Color color, byte ledIndex)
        {
            base.SetLed(Color.FromArgb(255, color.G, color.R, color.B), ledIndex); //Have to switch Red and Green channel
        }

        private bool _changingColor = false;
        public override void Apply()
        {
            if (!_changingColor)
            {
                _changingColor = true;
                SetColorToDLed();
                base.Apply();
                _changingColor = false;
            }
        }

        private void SetColorToDLed()
        {
            var x1 = _newLedData;
            var x2 = _newLedData.Skip(0);
            var x3 = _newLedData.Skip(0).Take(32 * 3).ToArray();
            _outputLED0.Invoke(_lstM97[0], new object[] { _newLedData.Skip(0).Take(32 * 3).ToArray() }); // bottom
            _outputLED1.Invoke(_lstM97[0], new object[] { _newLedData.Skip(32 * 3).Take(32 * 3).ToArray() }); // top
        }

        public void ResizeCtrlLen(int nLeds)
        {
            _nData0 = nLeds * 3;
            _times0 = _nData0 / 57;
            _iLed0 = _nData0 % 57;

            _nData1 = nLeds * 3;
            _times1 = _nData1 / 57;
            _iLed1 = _nData1 % 57;

            typeof(MCU_8297).GetField("times1", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_lstM97[0], _times0);
            typeof(MCU_8297).GetField("iLed1", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_lstM97[0], _iLed0);

            typeof(MCU_8297).GetField("times2", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_lstM97[0], _times1);
            typeof(MCU_8297).GetField("iLed2", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_lstM97[0], _iLed1);
        }

        public override void Shutdown()
        {
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            SetColorToDLed();
        }

        protected override void ConfirmApply()
        {
            SetColorToDLed();
        }
    }
}
