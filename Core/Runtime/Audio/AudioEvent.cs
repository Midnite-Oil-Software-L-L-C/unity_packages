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
        [SerializeField] [MinMaxRange(0, 1)] RangedFloat _volume = new RangedFloat(1, 1);
        [SerializeField] [MinMaxRange(0, 2f)] RangedFloat _pitch = new RangedFloat(1, 1);
        [SerializeField] [MinMaxRange(0f, 1000f)] RangedFloat _distance = new RangedFloat(1, 1000);
        
        public void Play(AudioSource audioSource)
        {
            audioSource.outputAudioMixerGroup = _audioMixerGroup;
            audioSource.volume = Random.Range(_volume._minValue, _volume._maxValue);
            audioSource.pitch = Random.Range(_pitch._minValue, _pitch._maxValue);
            audioSource.minDistance = _distance._minValue;
            audioSource.maxDistance = _distance._maxValue;
            var clipIndex = UnityEngine.Random.Range(0, _audioClips.Length);
            audioSource.PlayOneShot(_audioClips[clipIndex]);
        }
    }
}
