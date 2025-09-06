using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace MidniteOilSoftware.Core.Music
{
    [CreateAssetMenu(fileName = "MusicMix", menuName = "Midnite Oil Software/Core/Create Music Mix")]
    public class MusicMix : ScriptableObject
    {
        [SerializeField] AudioMixerGroup _mixerGroup;
        [SerializeField] AudioMixerSnapshot _audioMixerSnapshot;

        AudioSource _audioSource;
        Coroutine _fadeCoroutine;
        MusicManager _musicManager;

        public void Initialize(MusicManager instance)
        {
            _musicManager = instance;
            var audioSourceGameObject = new GameObject($"{name} Audio Source");
            audioSourceGameObject.transform.SetParent(_musicManager.transform);
            _audioSource = audioSourceGameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 1f;
            _audioSource.outputAudioMixerGroup = _mixerGroup;
        }

        public void PlayClip(AudioClip clip)
        {
            if (_fadeCoroutine != null)
            {
                MusicManager.Instance.StopCoroutine(_fadeCoroutine);
            }
            _audioSource.clip = clip;
            _audioSource.Play();
            _audioMixerSnapshot.TransitionTo(1f);
        }

        public void Stop()
        {
            _audioSource.Stop();
        }

        /// <summary>
        /// Fades out the audio volume over a specified duration using linear interpolation.
        /// </summary>
        /// <param name="duration">The time in seconds over which to fade out the audio.</param>
        /// <returns>A reference to the running Coroutine.</returns>
        public Coroutine FadeOut(float duration)
        {
            // Stop any existing fade coroutine to prevent conflicts.
            if (_fadeCoroutine != null)
            {
                MusicManager.Instance.StopCoroutine(_fadeCoroutine);
            }
            // Start the new fade coroutine and store its reference.
            _fadeCoroutine = MusicManager.Instance.StartCoroutine(FadeOutCoroutine(duration));
            return _fadeCoroutine;
        }

        private IEnumerator FadeOutCoroutine(float duration)
        {
            // Get the starting volume of the audio source.
            float startVolume = _audioSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // Increment elapsed time by the time since the last frame.
                elapsedTime += Time.deltaTime;
                // Calculate the interpolation value (t) between 0 and 1.
                float t = elapsedTime / duration;
                // Use Lerp to smoothly decrease the volume from the start volume to 0.
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                // Wait for the next frame before continuing the loop.
                yield return null;
            }

            // Ensure the volume is exactly 0 at the end.
            _audioSource.volume = 0f;
            // Stop the audio source after the fade is complete.
            _audioSource.Stop();
            // Clear the coroutine reference.
            _fadeCoroutine = null;
        }
    }
}
