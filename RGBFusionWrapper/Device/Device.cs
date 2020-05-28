using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace RGBFusionCli.Device
{
    public abstract class Device : IDevice
    {
        protected HashSet<int> _ledIndexes = new HashSet<int>();
        protected HashSet<int> _ignoreLedIndexes = new HashSet<int>();
        protected bool _transactionStarted = false;
        protected byte[] _currentLedData;
        protected byte[] _newLedData;
        protected DeviceType _deviceType = DeviceType.Unknown;

        public HashSet<int> IgnoreLedIndexes { get => _ignoreLedIndexes; set => _ignoreLedIndexes = value; }

        public HashSet<int> GetAreaIndexes()
        {
            return _ledIndexes;
        }

        public bool LedDataChanged()
        {
            return !(_newLedData.SequenceEqual(_currentLedData));
        }

        public bool LedIndexIsValid(int ledIndex)
        {
            return _ledIndexes.Contains(ledIndex);
        }

        public virtual void Init()
        {
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
            _transactionStarted = false;
            Thread.Sleep(5);
        }

        public void Cancel()
        {
            Array.Copy(_currentLedData, _newLedData, _currentLedData.Length);
            _transactionStarted = false;
        }

        public virtual DeviceType GetDeviceType()
        {
            return _deviceType;
        }

        public bool AddLedIndexToIgnoreList(int ledIndex)
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

        public bool RemoveLedIndexToIgnoreList(int ledIndex)
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
    }

}
