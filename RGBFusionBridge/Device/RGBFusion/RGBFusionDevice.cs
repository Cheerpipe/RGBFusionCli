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
     //     _RGBFusionController._ledFun.Set_Ezmode_Direct(LedLib2.LedMode.StillMode, Color.FromArgb(0, 0, 0, 0));
   //       _RGBFusionController._ledFun.Easy_mode_Apply(LedLib2.LedMode.StillMode, Color.FromArgb(0, 0, 0, 0), false);
        }

        public override HashSet<byte> GetDeviceIndexes()
        {
            return _RGBFusionController.Areas.Keys.ToHashSet();
        }

        public override void Init()
        {
            _deviceType = DeviceType.RGBFusion;
            _ledIndexes = new HashSet<byte>();
            ConfirmLastCommandTimeOut = 15;

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
                if (area.Pattern_info.But_Args[0].Color == Color_To_Int(255, _newLedData[3 * areaIndex], _newLedData[3 * areaIndex + 1], _newLedData[3 * areaIndex + 2]))
                    continue;

                Color newColor = Color.FromArgb(255, _newLedData[3 * areaIndex], _newLedData[3 * areaIndex + 1], _newLedData[3 * areaIndex + 2]);
                SolidColorBrush solidColorBrush = new SolidColorBrush(newColor);
                area.Pattern_info.Type = 0;
                area.Pattern_info.Bri = 9;
                area.Pattern_info.Speed = 2;
                area.Pattern_info.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(solidColorBrush);
                applyAreaClasses.Add(area);
            }
            return applyAreaClasses;
        }

        public uint Color_To_Int(Color color)
        {
            return (uint)((int)color.A << 24 | (int)color.R << 16 | (int)color.G << 8 | (int)color.B);
        }

        public uint Color_To_Int(int A, int R, int G, int B)
        {
            return (uint)((int)A << 24 | (int)R << 16 | (int)G << 8 | (int)B);
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

        byte _directInitializedCount = 0;
        private void SendColorToRGBFusion()
        {
            GetAreaClassesFromLedData();
            //Workarround to initialize leds in Z490 Aorus Master

        //  if (_directInitializedCount > 10)
         // {
                 _RGBFusionController.RGBFusion.Set_Adv_mode(applyAreaClasses, true);
       //     }
       //     else
       //     {
          //    _RGBFusionController.RGBFusion.Set_Adv_mode(applyAreaClasses, false);
         // //      _directInitializedCount++;
           // }
        }

        private bool _terminateDeviceThread = false;
        public override void Shutdown()
        {
            _terminateDeviceThread = true;
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            _RGBFusionControllerApplyEvent.Set();
        }

        protected override void ConfirmApply()
        {
            _RGBFusionControllerApplyEvent.Set();
        }
    }
}
