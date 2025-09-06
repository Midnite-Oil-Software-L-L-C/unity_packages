using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MidniteOilSoftware.Core.Audio
{
    [CreateAssetMenu(fileName = "Audio Event", menuName = "Midnite Oil Software/Core/Create Audio Event")]
    public class AudioEvent : ScriptableObject
    {
        [SerializeField] AudioMixerGroup _audioMixerGroup;
        [SerializeField] AudioClip[] _audioClips = Array.Empty<AudioClip>();
        [SerializeField] [MinMaxRange(0, 1)] RangedFloat _volume = new(1, 1);
        [SerializeField] [MinMaxRange(0, 2f)] RangedFloat _pitch = new(1, 1);

        [SerializeField] [MinMaxRange(0f, 1000f)]
        RangedFloat _distance = new(1, 1000);

        [Header("Debug Settings")]
        [SerializeField] bool _debugMode = false;

        void OnEnable()
        {
#if !UNITY_EDITOR
            _debugMode = false;
#endif
        }

        public void Play(AudioSource audioSource, float? volumeOverride = null)
        {
            if (_debugMode)
            {
                if (!_audioMixerGroup) Debug.LogError($"AudioMixerGroup is not assigned in AudioEvent: {name}", this);
                Debug.Log($"Playing audio event: {name} with volume override: {volumeOverride}", this);
            }
            audioSource.outputAudioMixerGroup = _audioMixerGroup;
            audioSource.volume = volumeOverride ?? Random.Range(_volume._minValue, _volume._maxValue);
            audioSource.pitch = Random.Range(_pitch._minValue, _pitch._maxValue);
            audioSource.minDistance = _distance._minValue;
            audioSource.maxDistance = _distance._maxValue;
            var clipIndex = Random.Range(0, _audioClips.Length);
            audioSource.PlayOneShot(_audioClips[clipIndex]);
        }
    }
}