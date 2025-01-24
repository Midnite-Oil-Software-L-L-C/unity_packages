using UnityEngine;
using UnityEngine.Audio;

namespace MidniteOilSoftware.Core.Settings
{
    public class AudioSettings : SingletonMonoBehaviour<AudioSettings>
    {
        [SerializeField] AudioMixer _audioMixer;

        public const string MasterVolumeKey = "MasterVolume";
        public const string MusicVolumeKey = "MusicVolume";
        public const string SfxVolumeKey = "SfxVolume";

        public void LoadAudioSettings()
        {
            var masterVolume = SettingsManager.Instance.GetSetting<float>(MasterVolumeKey, 1f);
            var musicVolume = SettingsManager.Instance.GetSetting<float>(MusicVolumeKey, 1f);
            var sfxVolume = SettingsManager.Instance.GetSetting<float>(SfxVolumeKey, 1f);

            _audioMixer.SetFloat(MasterVolumeKey, masterVolume);
            _audioMixer.SetFloat(MusicVolumeKey, musicVolume);
            _audioMixer.SetFloat(SfxVolumeKey, sfxVolume);
        }

        public void SaveAudioSettings(float masterVolume, float musicVolume, float sfxVolume)
        {
            SettingsManager.Instance.SetSetting<float>(MasterVolumeKey, masterVolume);
            SettingsManager.Instance.SetSetting<float>(MusicVolumeKey, musicVolume);
            SettingsManager.Instance.SetSetting<float>(SfxVolumeKey, sfxVolume, true);
        }
        
        public float MasterVolume => _audioMixer.GetFloat(MasterVolumeKey, out var volume);
        public float MusicVolume => _audioMixer.GetFloat(MusicVolumeKey, out var volume);
        public float SfxVolume => _audioMixer.GetFloat(SfxVolumeKey, out var volume);
        
        public void SetMasterVolume(float volume, bool saveSettings = false)
        {
            _audioMixer.SetFloat(MasterVolumeKey, volume);
            SettingsManager.Instance.SetSetting<float>(MasterVolumeKey, volume, saveSettings);
        }
        
        public void SetMusicVolume(float volume, bool saveSettings = false)
        {
            _audioMixer.SetFloat(MusicVolumeKey, volume);
            SettingsManager.Instance.SetSetting<float>(MusicVolumeKey, volume, saveSettings);
        }
        
        public void SetSfxVolume(float volume, bool saveSettings = false)
        {
            _audioMixer.SetFloat(SfxVolumeKey, volume);
            SettingsManager.Instance.SetSetting<float>(SfxVolumeKey, volume, saveSettings);
        }
    }
}