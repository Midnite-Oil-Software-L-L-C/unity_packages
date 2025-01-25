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
            MasterVolume = SettingsManager.Instance.GetSetting<float>(MasterVolumeKey, 1f);
            MusicVolume = SettingsManager.Instance.GetSetting<float>(MusicVolumeKey, 1f);
            SfxVolume = SettingsManager.Instance.GetSetting<float>(SfxVolumeKey, 1f);

            _audioMixer.SetFloat(MasterVolumeKey, MasterVolume);
            _audioMixer.SetFloat(MusicVolumeKey, MusicVolume);
            _audioMixer.SetFloat(SfxVolumeKey, SfxVolume);
        }

        public void SaveAudioSettings()
        {
            SettingsManager.Instance.SetSetting<float>(MasterVolumeKey, MasterVolume);
            SettingsManager.Instance.SetSetting<float>(MusicVolumeKey, MusicVolume);
            SettingsManager.Instance.SetSetting<float>(SfxVolumeKey, SfxVolume, true);
        }
        
        public void SetMasterVolume(float volume, bool saveSettings = false)
        {
            MasterVolume = volume;
            _audioMixer.SetFloat(MasterVolumeKey, MasterVolume);
            SettingsManager.Instance.SetSetting<float>(MasterVolumeKey, MasterVolume, saveSettings);
        }
        
        public void SetMusicVolume(float volume, bool saveSettings = false)
        {
            MusicVolume = volume;
            _audioMixer.SetFloat(MusicVolumeKey, MusicVolume);
            SettingsManager.Instance.SetSetting<float>(MusicVolumeKey, MusicVolume, saveSettings);
        }
        
        public void SetSfxVolume(float volume, bool saveSettings = false)
        {
            SfxVolume = volume;
            _audioMixer.SetFloat(SfxVolumeKey, SfxVolume);
            SettingsManager.Instance.SetSetting<float>(SfxVolumeKey, SfxVolume, saveSettings);
        }
    }
}