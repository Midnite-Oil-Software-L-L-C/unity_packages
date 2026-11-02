using UnityEngine;
using UnityEngine.Audio;

namespace MidniteOilSoftware.Core.Settings
{
    public class AudioSettings : SingletonMonoBehaviour<AudioSettings>
    {
        [SerializeField] AudioMixer _audioMixer;

        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }

        const string MasterVolumeKey = "MasterVolume";
        const string MusicVolumeKey = "MusicVolume";
        const string SfxVolumeKey = "SfxVolume";

        public void LoadAudioSettings()
        {
            if (!_audioMixer) return;
            MasterVolume = LoadVolume(MasterVolumeKey);
            MusicVolume = LoadVolume(MusicVolumeKey);
            SfxVolume = LoadVolume(SfxVolumeKey);
        }

        public void SaveAudioSettings()
        {
            SettingsManager.Instance.SetSetting<float>(MasterVolumeKey, MasterVolume);
            SettingsManager.Instance.SetSetting<float>(MusicVolumeKey, MusicVolume);
            SettingsManager.Instance.SetSetting<float>(SfxVolumeKey, SfxVolume, true);
        }

        public void SetMasterVolume(float volume, bool saveSettings = false) =>
            MasterVolume = SetVolume(MasterVolumeKey, volume, saveSettings);

        public void SetMusicVolume(float volume, bool saveSettings = false) =>
            MusicVolume = SetVolume(MusicVolumeKey, volume, saveSettings);

        public void SetSfxVolume(float volume, bool saveSettings = false) =>
            SfxVolume = SetVolume(SfxVolumeKey, volume, saveSettings);

        // Exposed mixer parameters are in decibels. Only override the mixer when the player has saved a value;
        // otherwise keep the level authored in the mixer's start snapshot. Calling SetFloat with a made-up default
        // would permanently replace the authored level.
        float LoadVolume(string key)
        {
            var authoredVolume = _audioMixer.GetFloat(key, out var currentVolume) ? currentVolume : 0f;
            var savedVolume = SettingsManager.Instance.GetSetting<float>(key, float.NaN);
            if (float.IsNaN(savedVolume)) return authoredVolume;
            _audioMixer.SetFloat(key, savedVolume);
            return savedVolume;
        }

        float SetVolume(string key, float volume, bool saveSettings)
        {
            if (_audioMixer) _audioMixer.SetFloat(key, volume);
            SettingsManager.Instance.SetSetting<float>(key, volume, saveSettings);
            return volume;
        }
    }
}
