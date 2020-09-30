using LedLib2;
using SelLEDControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using LedLib2.IT8297;
using System.Drawing;
using System.Threading;

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
            for (byte l = 0; l < 81; l++)
                _ledIndexes.Add(l);

            base.Init();


            _lstM97[0].init_dstrip_direct_control(1, _ledIndexes.Count);
            //_lstM97[0].init_dstrip_direct_control(2, _ledIndexes.Count);



            //            _lstM97[0].StopEffectThread();

            //  _lstM97[0].SetLedEffect(LMode_8297.Static, 22222222, 0, 9, 6, true);

            //_lstM97[0].init_dstrip_direct_control(1, _ledIndexes.Count);
            //_lstM97[0].init_dstrip_direct_control(2, _ledIndexes.Count);

            //_lstM97[0].TurnOff_DStrip();

            // _lstM97[0].init_dstrip_direct_control(0, 32); //esto arregla el canal 0 para que no parpadee
            // _lstM97[0].init_dstrip_direct_control(1,32);
            //_lstM97[0].init_dstrip_direct_control(2, 32);
            // _ledObject.InitDStripDirectCtrl(0, 32);
            //_ledObject.InitDStripDirectCtrl(1, _ledIndexes.Count);
            //_ledObject.DStripDirectControlStart(1);
            // _ledObject.InitDStripDirectCtrl(2, 32);

            ResizeCtrlLen(_ledIndexes.Count);

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
            _outputLED0.Invoke(_lstM97[0], new object[] { _newLedData });
            //  _outputLED1.Invoke(_lstM97[0], new object[] { _newLedData });
            Thread.Sleep(2);
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
