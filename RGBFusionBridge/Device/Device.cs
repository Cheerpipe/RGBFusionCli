using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace RGBFusionBridge.Device
{
    public abstract class Device : IDevice
    {
        protected HashSet<byte> _ledIndexes = new HashSet<byte>();
        protected HashSet<byte> _ignoreLedIndexes = new HashSet<byte>();
        protected byte[] _currentLedData;
        protected byte[] _newLedData;
        protected DeviceType _deviceType = DeviceType.Unknown;
        protected Timer _confirmLastCommandTimer;
        public bool ConfirmLastCommand { get; set; } = true;
        public int ConfirmLastCommandTimeOut { get; set; } = 200;

        public HashSet<byte> IgnoreLedIndexes { get => _ignoreLedIndexes; set => _ignoreLedIndexes = value; }

        public HashSet<byte> GetAreaIndexes()
        {
            return _ledIndexes;
        }

        private void ConfirmLastCommandTimerTick(object state)
        {
            StopConfirmCommandTimer();
            ConfirmApply();
        }

        protected abstract void ConfirmApply();

        public bool LedDataChanged()
        {
            return !(_newLedData.SequenceEqual(_currentLedData));
        }

        public bool LedIndexIsValid(byte ledIndex)
        {
            return _ledIndexes.Contains(ledIndex) || ledIndex == 255;
        }

        public virtual void Init()
        {
            _confirmLastCommandTimer = new Timer(new TimerCallback(ConfirmLastCommandTimerTick), null, Timeout.Infinite, Timeout.Infinite);
            _currentLedData = new byte[_ledIndexes.Count * 3];
            _newLedData = new byte[_ledIndexes.Count * 3];
            if (_deviceType == DeviceType.Unknown)
                throw new Exception("DeviceType is Unknown. You must set it in the chils class");
        }

        public virtual void SetLed(Color color, byte ledIndex)
        {
            if (!LedIndexIsValid(ledIndex))
                throw new ArgumentOutOfRangeException("Led index is out for this device.");

            if (_ignoreLedIndexes.Contains(ledIndex))
                return;
            _newLedData[3 * ledIndex] = color.R;
            _newLedData[3 * ledIndex + 1] = color.G;
            _newLedData[3 * ledIndex + 2] = color.B;
        }

        public virtual void Apply()
        {
            Array.Copy(_newLedData, _currentLedData, _currentLedData.Length);
            Thread.Sleep(1);
            RestartConfirmCommandTimer();
        }

        protected void StartConfirmCommandTimer()
        {
            _confirmLastCommandTimer.Change(ConfirmLastCommandTimeOut, ConfirmLastCommandTimeOut);
        }

        protected void StopConfirmCommandTimer()
        {
            _confirmLastCommandTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected void RestartConfirmCommandTimer()
        {
            StopConfirmCommandTimer();
            StartConfirmCommandTimer();
        }

        public void Cancel()
        {
            Array.Copy(_currentLedData, _newLedData, _currentLedData.Length);
        }

        public virtual DeviceType GetDeviceType()
        {
            return _deviceType;
        }

        public bool AddLedIndexToIgnoreList(byte ledIndex)
        {
            try
            {
                _ignoreLedIndexes.Add(ledIndex);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveLedIndexToIgnoreList(byte ledIndex)
        {
            try
            {
                _ignoreLedIndexes.Remove(ledIndex);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public abstract void Shutdown();
    }
}
