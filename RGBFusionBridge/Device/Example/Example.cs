
namespace RGBFusionBridge.Device.ExampleDevice
{
    public class ExampleDevice : Device
    {
        private bool _changingColor = false;

        public override void Init()
        {
            _deviceType = DeviceType.Unknown;
            base.Init();
        }
        public override void Apply()
        {
            if (!_changingColor)
            {
                _changingColor = true;
                base.Apply();
                _changingColor = false;
            }
        }
        public override void Shutdown()
        {
            for (int p = 0; p < _newLedData.Length; p++) { _newLedData[p] = 0; }
            Apply();
        }

        protected override void ConfirmApply()
        {
        }
    }
}
