using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace MidniteOilSoftware.Core.Music
{
    /// <summary>
    /// One music "deck" (AudioSource routed to a mixer group). MusicManager crossfades between two of these.
    /// Fades are driven by AudioSource.volume rather than AudioMixer snapshots because the Web (WebGL)
    /// platform does not support snapshot transitions — only group volume.
    /// </summary>
    [CreateAssetMenu(fileName = "MusicMix", menuName = "Midnite Oil Software/Core/Create Music Mix")]
    public class MusicMix : ScriptableObject
    {
        [SerializeField] AudioMixerGroup _mixerGroup;
        [Tooltip("AudioSource volume (0-1) this mix plays at. Use the mixer group to set the level in dB.")]
        [SerializeField, Range(0f, 1f)] float _volume = 1f;

        AudioSource _audioSource;
        Coroutine _fadeCoroutine;
        MusicManager _musicManager;

        public bool IsPlaying => _audioSource && _audioSource.isPlaying;

        public void Initialize(MusicManager instance)
        {
            _musicManager = instance;
            _fadeCoroutine = null;
            var audioSourceGameObject = new GameObject($"{name} Audio Source");
            audioSourceGameObject.transform.SetParent(_musicManager.transform);
            _audioSource = audioSourceGameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = _volume;
            _audioSource.outputAudioMixerGroup = _mixerGroup;
        }

        /// <summary>
        /// Plays the clip on this mix, optionally fading in from silence.
        /// </summary>
        public void PlayClip(AudioClip clip, float fadeInTime = 0f)
        {
            if (!_audioSource) return;
            StopFade();
            _audioSource.clip = clip;
            _audioSource.volume = fadeInTime > 0f ? 0f : _volume;
            _audioSource.Play();
            if (fadeInTime <= 0f) return;
            _fadeCoroutine = _musicManager.StartCoroutine(FadeCoroutine(_volume, fadeInTime, false));
        }

        public void Stop()
        {
            if (!_audioSource) return;
            StopFade();
            _audioSource.Stop();
            _audioSource.volume = _volume;
        }

        /// <summary>
        /// Fades the audio out over the given duration, then stops the AudioSource.
        /// </summary>
        /// <param name="duration">The time in seconds over which to fade out the audio.</param>
        /// <returns>The running Coroutine, or null if nothing needed to fade.</returns>
        public Coroutine FadeOut(float duration)
        {
            if (!IsPlaying) return null;
            StopFade();
            if (duration <= 0f)
            {
                Stop();
                return null;
            }

            _fadeCoroutine = _musicManager.StartCoroutine(FadeCoroutine(0f, duration, true));
            return _fadeCoroutine;
        }

        void StopFade()
        {
            if (_fadeCoroutine == null) return;
            if (_musicManager) _musicManager.StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        // Unscaled time so fades still complete while the game is paused (Time.timeScale = 0).
        IEnumerator FadeCoroutine(float targetVolume, float duration, bool stopWhenDone)
        {
            var startVolume = _audioSource.volume;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
                yield return null;
            }

            _audioSource.volume = targetVolume;
            _fadeCoroutine = null;
            if (!stopWhenDone) yield break;
            _audioSource.Stop();
            _audioSource.volume = _volume;
        }
    }
}
