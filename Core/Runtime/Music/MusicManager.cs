using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AudioSettings = MidniteOilSoftware.Core.Settings.AudioSettings;

namespace MidniteOilSoftware.Core.Music
{
    public class MusicManager : SingletonMonoBehaviour<MusicManager>
    {
        [SerializeField] List<MusicClipGroup> _musicClipGroups;
        [SerializeField] List<MusicMix> _musicMixes;
        [Tooltip("Seconds the outgoing track fades out while the incoming track fades in.")]
        [SerializeField, Min(0f)] float _crossfadeTime = 1f;

        int _currentMixIndex;
        [SerializeField] MusicClipGroup _currentMusicGroup;
        Timer _nextTrackTimer;

        Action<StopAllMusicEvent> _stopAllMusicAction;

        bool _musicEnabled = true;
        bool _loop;

        public bool MusicEnabled
        {
            get => _musicEnabled;
            set
            {
                if (_musicEnabled == value) return;
                _musicEnabled = value;
                if (!_musicEnabled) StopAllMusic();
            }
        }

        bool HasMusicMixes => _musicMixes is { Count: > 0 };

#if UNITY_EDITOR
        [ContextMenu("Play menu music")]
        public void PlayMenuMusic() => PlayMusic("Menu Music");

        [ContextMenu("Play game music")]
        public void PlayGameMusic() => PlayMusic("Game Music");

        [ContextMenu("Play game over music")]
        public void PlayGameOverMusic() => PlayMusic("Game Over Music");

        [ContextMenu("Stop all music")]
        public void StopMusic() => StopAllMusic();

        [ContextMenu("Fade out music")]
        public void FadeOutMusicCommand() => FadeOutMusic();
#endif

        public void PlayMusic(string musicGroupName, bool loop = false)
        {
            if (!MusicEnabled) return;
            var musicGroup = GetMusicGroup(musicGroupName);
            if (!musicGroup)
            {
                Debug.LogWarning($"MusicManager: no MusicClipGroup named '{musicGroupName}'. Keeping current music.", this);
                return;
            }

            StopNextTrackTimer();
            _currentMusicGroup = musicGroup;
            _loop = loop;
            PlayNextTrack();
        }

        public void StopAllMusic()
        {
            StopNextTrackTimer();
            if (!HasMusicMixes) return;
            foreach (var musicMix in _musicMixes) musicMix.Stop();
        }

        public void FadeOutMusic(float fadeOutTime = 1f)
        {
            if (!HasMusicMixes) return;
            StopNextTrackTimer();
            _musicMixes[_currentMixIndex].FadeOut(fadeOutTime);
        }

        public void EnableMusic(bool musicEnabled = true)
        {
            if (!musicEnabled)
            {
                MusicEnabled = false;
                return;
            }

            if (MusicEnabled) return;
            MusicEnabled = true;
            PlayNextTrack();
        }

        // Crossfades: the outgoing mix fades out while the incoming mix fades in. The outgoing track is always
        // stopped, so nothing depends on AudioMixer snapshots (unsupported on WebGL) to silence it.
        void PlayNextTrack()
        {
            if (!MusicEnabled || !HasMusicMixes || !_currentMusicGroup) return;
            var clip = _currentMusicGroup.GetNextMusicClip();
            if (!clip) return;
            _musicMixes[_currentMixIndex].FadeOut(_crossfadeTime);
            ToggleCurrentMix();
            _musicMixes[_currentMixIndex].PlayClip(clip, _crossfadeTime);
            if (!_loop) StartNextTrackTimer(clip);
        }

        void ToggleCurrentMix() => _currentMixIndex = (_currentMixIndex + 1) % _musicMixes.Count;

        MusicClipGroup GetMusicGroup(string musicGroupName) =>
            _musicClipGroups?.FirstOrDefault(group => group && group.name == musicGroupName);

        void InitializeMusicMixes()
        {
            AudioSettings.Instance.LoadAudioSettings();
            if (!HasMusicMixes) return;
            foreach (var musicMix in _musicMixes) musicMix.Initialize(Instance);
        }

        protected override void Awake()
        {
            base.Awake();
            _stopAllMusicAction = _ => StopAllMusic();
        }

        IEnumerator Start()
        {
            yield return null;
            CreateNextTrackTimer();
            SubscribeToEvents();
            InitializeMusicMixes();
        }

        void OnDisable()
        {
            UnsubscribeFromEvents();
            ReleaseNextTrackTimer();
        }

        void CreateNextTrackTimer() => _nextTrackTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();

        // Start the crossfade so it finishes as the current clip ends.
        void StartNextTrackTimer(AudioClip clip)
        {
            if (_nextTrackTimer == null) return;
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            _nextTrackTimer.OnTimerStop += PlayNextTrack;
            _nextTrackTimer.Start(Mathf.Max(0f, clip.length - _crossfadeTime));
        }

        void StopNextTrackTimer()
        {
            if (_nextTrackTimer == null) return;
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            _nextTrackTimer.Stop();
        }

        void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<PlayMusicEvent>(OnPlayMusicEvent);
            EventBus.Instance.Subscribe<StopAllMusicEvent>(_stopAllMusicAction);
        }

        void UnsubscribeFromEvents()
        {
            EventBus.Instance?.Unsubscribe<PlayMusicEvent>(OnPlayMusicEvent);
            EventBus.Instance?.Unsubscribe<StopAllMusicEvent>(_stopAllMusicAction);
        }

        void ReleaseNextTrackTimer()
        {
            if (_nextTrackTimer == null) return;
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_nextTrackTimer);
        }

        void OnPlayMusicEvent(PlayMusicEvent playMusicEvent) =>
            PlayMusic(playMusicEvent.MusicGroupName, playMusicEvent.Loop);
    }
}
