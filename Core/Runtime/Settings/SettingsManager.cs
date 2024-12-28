using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MidniteOilSoftware.Core.Settings
{
    public class SettingsManager : SingletonMonoBehaviour<SettingsManager>
    {
        public string GetSetting(string key, string defaultValue)
        {
            _settings.TryAdd(key, defaultValue);
            return _settings[key];
        }

        public int GetSetting<TInt32>(string key, int defaultValue)
        {
            if (!_settings.TryGetValue(key, out var value)) return defaultValue;
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public float GetSetting<TFloat>(string key, float defaultValue)
        {
            if (!_settings.TryGetValue(key, out var value)) return defaultValue;
            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        public void SetSetting(string key, string value, bool save = false)
        {
            _settings[key] = value;
            if (save) SaveSettings();
        }

        public void SetSetting<T>(string key, T value, bool save = false)
        {
            _settings[key] = value.ToString();
            if (save) SaveSettings();
        }

        readonly Dictionary<string, string> _settings = new();
        const string SettingsKey = "PreferencKeys";

        protected override void Awake()
        {
            base.Awake();
            LoadSettings();
        }

        void LoadSettings()
        {
            var keys = PlayerPrefs.GetString(SettingsKey, string.Empty).Split(',');
            foreach (var key in keys)
            {
                var value = PlayerPrefs.GetString(key, string.Empty);
                _settings.TryAdd(key, value);
            }
        }

        void SaveSettings()
        {
            StringBuilder sb = new();
            foreach (var key in _settings.Keys)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(key);
                PlayerPrefs.SetString(key, _settings[key]);
            }

            PlayerPrefs.SetString(SettingsKey, sb.ToString());
            PlayerPrefs.Save();
            EventBus.Instance?.Raise(new SettingsSavedEvent());
        }
    }
}