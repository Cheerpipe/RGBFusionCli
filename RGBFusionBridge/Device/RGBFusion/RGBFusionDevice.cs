using SelLEDControl;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace RGBFusionBridge.Device.RGBFusion
{
    public class RGBFusionDevice : Device
    {
        private RGBFusionLoader _RGBFusionController;
        private Dictionary<byte, CommUI.Area_class> _allAreaInfo = new Dictionary<byte, CommUI.Area_class>();
        private Thread _RGBFusionControllerThread;
        private readonly ManualResetEvent _RGBFusionControllerApplyEvent = new ManualResetEvent(false);

        public RGBFusionDevice(RGBFusionLoader rgbFusionController, bool setAllAreasWithRGBFusion)
        {
            _RGBFusionController = rgbFusionController;
            _allAreaInfo = _RGBFusionController.Areas;
        }

        public override HashSet<byte> GetDeviceIndexes()
        {
            return _RGBFusionController.Areas.Keys.ToHashSet();
        }

        public override void Init()
        {
            _deviceType = DeviceType.RGBFusion;

            _ledIndexes = new HashSet<byte>();


            for (byte i = 0; i <= _RGBFusionController.Areas.Keys.Max(); i++)
            {
                _ledIndexes.Add(i);
            }

            _RGBFusionControllerThread = new Thread(SetMainboardRingAreas);
            _RGBFusionControllerThread.SetApartmentState(ApartmentState.STA);
            _RGBFusionControllerThread.Start();
            base.Init();
        }

        public override void SetLed(System.Drawing.Color color, byte ledIndex)
        {
            if (ledIndex == 255)
            {
                foreach (byte led in _ledIndexes)
                {
                    base.SetLed(color, led);
                }
            }
            else
            {
                base.SetLed(color, ledIndex);
            }
        }

        private List<CommUI.Area_class> GetAreaClassesFromLedData()
        {
            applyAreaClasses.Clear();
            for (byte areaIndex = 0; areaIndex < _ledIndexes.Count; areaIndex++)
            {
                if (!_allAreaInfo.Keys.Contains(areaIndex) || _ignoreLedIndexes.Contains(areaIndex)) //Ignore intermediate area index that don't exists in the motherboard
                    continue;
                CommUI.Area_class area = _allAreaInfo[areaIndex];
                Color newColor = Color.FromArgb(255, _newLedData[3 * areaIndex], _newLedData[3 * areaIndex + 1], _newLedData[3 * areaIndex + 2]);
                SolidColorBrush solidColorBrush = new SolidColorBrush(newColor);
                area.Pattern_info.Type = 0;
                area.Pattern_info.Bri = 9;
                area.Pattern_info.Speed = -1;
                uint currentColor = area.Pattern_info.But_Args[0].Color; //Just a workarround
                area.Pattern_info.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(solidColorBrush);
                if (currentColor != area.Pattern_info.But_Args[0].Color) //Just a workarround
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
                SendColorToRGBFusion();
                base.Apply();
                _changingColor = false;
            }
        }

        private void SendColorToRGBFusion()
        {
            GetAreaClassesFromLedData();
            _RGBFusionController.RGBFusion.Set_Adv_mode(applyAreaClasses, true);
        }

        private bool _terminateDeviceThread = false;
        public override void Shutdown()
        {
            _terminateDeviceThread = true;
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }

        protected override void ConfirmApply()
        {
            SendColorToRGBFusion();
        }
    }
}
