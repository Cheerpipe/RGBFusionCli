using SelLEDControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace RGBFusionCli.Device.RGBFusion
{
    public class RGBFusionDevice : Device
    {
        private RGBFusionLoader _RGBFusionController;
        private bool _setAllAreasWithRGBFusion;
        private Dictionary<int, CommUI.Area_class> _allAreaInfo = new Dictionary<int, CommUI.Area_class>();
        private Thread _RGBFusionControllerThread;
        readonly ManualResetEvent _RGBFusionControllerApplyEvent = new ManualResetEvent(false);

        public RGBFusionDevice(RGBFusionLoader rgbFusionController, bool setAllAreasWithRGBFusion)
        {
            _RGBFusionController = rgbFusionController;
            _allAreaInfo = _RGBFusionController.Areas;
            _setAllAreasWithRGBFusion = setAllAreasWithRGBFusion;
        }

        public override void Init()
        {
            _deviceType = DeviceType.RGBFusion;
            if (_setAllAreasWithRGBFusion)
            {
                _ledIndexes = new HashSet<int>();
                for (int i = 0; i <= _allAreaInfo.Count; i++)
                {
                    _ledIndexes.Add(i);
                }
            }
            else
            {
                _ledIndexes.Add(0);
                _ledIndexes.Add(1);
                _ledIndexes.Add(2);
                _ledIndexes.Add(3);
                _ledIndexes.Add(4);
            }
            _RGBFusionControllerThread = new Thread(SetMainboardRingAreas);
            _RGBFusionControllerThread.SetApartmentState(ApartmentState.STA);
            _RGBFusionControllerThread.Start();
            base.Init();
        }

        private List<CommUI.Area_class> GetAreaClassesFromLedData()
        {
            applyAreaClasses.Clear();
            for (int areaIndex = 0; areaIndex < _ledIndexes.Count; areaIndex++)
            {
                if (!_allAreaInfo.Keys.Contains(areaIndex) || _ignoreLedIndexes.Contains(areaIndex)) //Ignore intermediate area index that don't exists in the motherboard
                    continue;
                CommUI.Area_class area = _allAreaInfo[areaIndex];
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(255, _newLedData[3 * areaIndex], _newLedData[3 * areaIndex + 1], _newLedData[3 * areaIndex + 2]));
                area.Pattern_info.Type = 0;
                area.Pattern_info.Bri = 9;
                area.Pattern_info.Speed = 2;
                area.Pattern_info.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(solidColorBrush);
                applyAreaClasses.Add(area);
            }
            return applyAreaClasses;
        }

        List<CommUI.Area_class> applyAreaClasses = new List<CommUI.Area_class>();
        public override void Apply()
        {
            _RGBFusionControllerApplyEvent.Set();
        }

        public void SetMainboardRingAreas()
        {
            do
            {
                _RGBFusionControllerApplyEvent.WaitOne();
                DoApply();
                _RGBFusionControllerApplyEvent.Reset();
            } while (_RGBFusionControllerThread.IsAlive && !_terminateDeviceThread);
        }

        private bool _changingColor = false;
        private void DoApply()
        {
            if (!_changingColor)
            {
                _changingColor = true;
                GetAreaClassesFromLedData();
                _RGBFusionController.RGBFusion.Set_Adv_mode(applyAreaClasses, true);
                base.Apply();
                _changingColor = false;
            }
        }
        private bool _terminateDeviceThread = false;
        public override void Shutdown()
        {
            _terminateDeviceThread = true;
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }
    }
}
