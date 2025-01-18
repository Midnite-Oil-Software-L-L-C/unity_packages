using System;

namespace MidniteOilSoftware.Core
{
    public class MinMaxRangeAttribute : Attribute
    {
        public float Max { get; set; }
        public float Min { get; set; }
        
        public MinMaxRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

    }
}
