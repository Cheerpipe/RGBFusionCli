using LedLib2;
using SelLEDControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using LedLib2.IT8297;
using System.Threading;
using System.Drawing;

namespace RGBFusionBridge.Device.DledPinHeader
{
    public class Z390DledPinHeaderDevice : Device
    {
        private RGBFusionLoader _RGBFusionController;
        private LedObject _ledObject;
        private DC_DLED _dledController;
        private Collection_8297 _coll97;
        public Z390DledPinHeaderDevice(RGBFusionLoader rgbFusionController)
        {
            _RGBFusionController = rgbFusionController;
            _ledObject = (LedObject)typeof(Comm_LED_Fun).GetField("LedObj", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_RGBFusionController._ledFun);
            _coll97 = (Collection_8297)typeof(LedObject).GetField("coll97", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_ledObject);
            _dledController = (DC_DLED)typeof(MCU_8297).GetField("dcStrip0", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_coll97.lstM97[0]);
        }


        public override void Init()
        {
            _deviceType = DeviceType.DledPinHeader;
            for (byte l = 0; l < 18; l++)
                _ledIndexes.Add(l);

            base.Init();
            _coll97.lstM97[0].Enable_DLedStripCtrl(0, true);
            _dledController.ResizeCtrlLen(_ledIndexes.Count);
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
            _dledController.OutputLED(_newLedData);
        }


        public override void Shutdown()
        {
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }

        protected override void ConfirmApply()
        {
            SetColorToDLed();
        }
    }
}
