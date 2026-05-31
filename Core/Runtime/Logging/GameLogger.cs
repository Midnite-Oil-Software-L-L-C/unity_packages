namespace MidniteOilSoftware.Core.Logging
{
    /// <summary>
    /// Zero-allocation-when-disabled logging wrapper.
    /// Generic overloads capture value types without boxing at the call site;
    /// string formatting and Debug.Log* calls only occur inside the IsEnabled guard.
    /// </summary>
    public static class GameLogger
    {
        public static bool IsEnabled { get; private set; } = true;

        [RuntimeInitializeOnLoadMethod]
        static void Initialize() => IsEnabled = true;

        public static void Enable(bool enable) => IsEnabled = enable;

        // ── Simple overloads (forward to Debug.Log*) ─────────────────────────────

        public static void Log(string message, Object context) 
        { 
            if (IsEnabled) Debug.Log(message, context); 
        }

        public static void Log(string message) { if (IsEnabled) Debug.Log(message); }
        public static void Log<T0>(string format, T0 arg0) { if (IsEnabled) Debug.Log(string.Format(format, arg0)); }
        public static void Log<T0, T1>(string format, T0 arg0, T1 arg1) { if (IsEnabled) Debug.Log(string.Format(format, arg0, arg1)); }
        public static void Log<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2) { if (IsEnabled) Debug.Log(string.Format(format, arg0, arg1, arg2)); }
        public static void Log<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3) { if (IsEnabled) Debug.Log(string.Format(format, arg0, arg1, arg2, arg3)); }

        public static void LogWarning(string message) { if (IsEnabled) Debug.LogWarning(message); }
        public static void LogWarning<T0>(string format, T0 arg0) { if (IsEnabled) Debug.LogWarning(string.Format(format, arg0)); }
        public static void LogWarning<T0, T1>(string format, T0 arg0, T1 arg1) { if (IsEnabled) Debug.LogWarning(string.Format(format, arg0, arg1)); }
        public static void LogWarning<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2) { if (IsEnabled) Debug.LogWarning(string.Format(format, arg0, arg1, arg2)); }

        public static void LogError(string message) { if (IsEnabled) Debug.LogError(message); }
        public static void LogError<T0>(string format, T0 arg0) { if (IsEnabled) Debug.LogError(string.Format(format, arg0)); }
        public static void LogError<T0, T1>(string format, T0 arg0, T1 arg1) { if (IsEnabled) Debug.LogError(string.Format(format, arg0, arg1)); }
        public static void LogError<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2) { if (IsEnabled) Debug.LogError(string.Format(format, arg0, arg1, arg2)); }

        // ── Logwin-style overloads (key + message/format + category) ─────────────

        public static void Log(string key, string message, string category)
        {
            if (!IsEnabled) return;
            Debug.Log($"{key};{message};{category}");
        }

        public static void Log<T0>(string key, string format, string category, T0 arg0)
        {
            if (!IsEnabled) return;
            var msg = string.Format(format, arg0);
            Debug.Log($"{key};{msg};{category}");
        }

        public static void Log<T0, T1>(string key, string format, string category, T0 arg0, T1 arg1)
        {
            if (!IsEnabled) return;
            var msg = string.Format(format, arg0, arg1);
            Debug.Log($"{key};{msg};{category}");
        }

        public static void LogWarning(string key, string message, string category)
        {
            if (!IsEnabled) return;
            Debug.LogWarning($"{key};{message};{category}");
        }

        public static void LogWarning<T0>(string key, string format, string category, T0 arg0)
        {
            if (!IsEnabled) return;
            var msg = string.Format(format, arg0);
            Debug.LogWarning($"{key};{msg};{category}");
        }

        public static void LogError(string key, string message, string category)
        {
            if (!IsEnabled) return;
            Debug.LogError($"{key};{message};{category}");
        }

        public static void LogError<T0>(string key, string format, string category, T0 arg0)
        {
            if (!IsEnabled) return;
            var msg = string.Format(format, arg0);
            Debug.LogError($"{key};{msg};{category}");
        }
    }
}
