using UnityEngine;

namespace MidniteOilSoftware.Core
{
    // Exclude this class if you have the real Logwin package
    public static class Logwin
    {
        public static void Log(string system, string message, string category = "General")
        {
            Debug.Log($"[{system}] [{category}] {message}");
        }

        public static void LogWarning(string system, string message, string category = "General")
        {
            Debug.LogWarning($"[{system}] [{category}] {message}");
        }

        public static void LogError(string system, string message, string category = "General")
        {
            Debug.LogError($"[{system}] [{category}] {message}");
        }
    }
}
