using System.Collections.Generic;
using UnityEngine;

namespace MidniteOilSoftware.Core
{
    public static class HelperFunctions 
    {    
        static readonly Dictionary<float, WaitForSeconds> WaitForSeconds = new();
    
        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            if (WaitForSeconds.TryGetValue(seconds, out var forSeconds)) return forSeconds;
            var waitForSeconds = new WaitForSeconds(seconds);
            WaitForSeconds.Add(seconds, waitForSeconds);
            return WaitForSeconds[seconds];
        }
        
        public static T SafeGetByIndex<T>(this List<T> list, int index)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            return default;
        }
    }
}