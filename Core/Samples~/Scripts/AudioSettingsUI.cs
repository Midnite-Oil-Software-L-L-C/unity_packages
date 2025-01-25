using UnityEngine;
using UnityEngine.UI;
using AudioSettings = MidniteOilSoftware.Core.Settings.AudioSettings;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] Slider _musicVolumeSlider, _sfxVolumeSlider, _masterVolumeSlider;

    void Start()
    {
        AudioSettings.Instance.LoadAudioSettings();
        InitializeSliders();
    }

    void OnDisable()
    {
        UnsubscribeFromSliderChanges();
    }

    void InitializeSliders()
    {
        _masterVolumeSlider.value = MixerToSliderValue(AudioSettings.Instance.MasterVolume);
        _musicVolumeSlider.value = MixerToSliderValue(AudioSettings.Instance.MusicVolume);
        _sfxVolumeSlider.value = MixerToSliderValue(AudioSettings.Instance.SfxVolume);
        SubscribeToSliderChanges();
    }

    void SubscribeToSliderChanges()
    {
        _musicVolumeSlider.onValueChanged.AddListener(MusicVolumeChanged);
        _sfxVolumeSlider.onValueChanged.AddListener(SfxVolumeChanged);
        _masterVolumeSlider.onValueChanged.AddListener(MasterVolumeChanged);
    }

    void UnsubscribeFromSliderChanges()
    {
        _musicVolumeSlider.onValueChanged.RemoveListener(MusicVolumeChanged);
        _sfxVolumeSlider.onValueChanged.RemoveListener(SfxVolumeChanged);
        _masterVolumeSlider.onValueChanged.RemoveListener(MasterVolumeChanged);
    }

    void MusicVolumeChanged(float sliderValue)
    {
        AudioSettings.Instance.SetMusicVolume(SliderToMixerValue(sliderValue), true);
    }

    void SfxVolumeChanged(float sliderValue)
    {
        AudioSettings.Instance.SetSfxVolume(SliderToMixerValue(sliderValue), true);
    }

    void MasterVolumeChanged(float sliderValue)
    {
        AudioSettings.Instance.SetMasterVolume(SliderToMixerValue(sliderValue), true);
    }
    
    static float MixerToSliderValue(float mixerValue)
    {
        return Mathf.InverseLerp(-80, 20, mixerValue);
    }

    static float SliderToMixerValue(float sliderValue)
    {
        return Mathf.Lerp(-80, 20, sliderValue);
    } 
}
