using System;

namespace MidniteOilSoftware.Core
{
    [Serializable]
    public struct RangedFloat
    {
        public float _minValue, _maxValue;
        public RangedFloat(float minValue, float maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }
    }
}
