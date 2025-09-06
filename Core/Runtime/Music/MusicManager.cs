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

        int _currentMixIndex;
        [SerializeField] MusicClipGroup _currentMusicGroup;
        Timer _nextTrackTimer;
        
        Action<StopAllMusicEvent> _stopAllMusicAction;

        public bool MusicEnabled { get; private set; }

#if UNITY_EDITOR
        [ContextMenu("Play menu music")]
        public void PlayMenuMusic()
        {
            PlayMusic("Main Menu Music");
        }

        [ContextMenu("Play game music")]
        public void PlayGameMusic()
        {
            PlayMusic("Game Play Music");
        }

        [ContextMenu("Play game over music")]
        public void PlayGameOverMusic()
        {
            PlayMusic("GameOver");
        }

        [ContextMenu("Stop all music")]
        public void StopMusic()
        {
            StopAllMusic();
        }

        [ContextMenu("Fade out music")]
        public void FadeOutMusicCommand()
        {
            FadeOutMusic();
        }
#endif

        public void PlayMusic(string musicGroupName)
        {
            if (!MusicEnabled) return;
            _currentMusicGroup = GetMusicGroup(musicGroupName);
            PlayNextTrack();
        }

        public void StopAllMusic()
        {
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            _nextTrackTimer.Stop();
            foreach (var musicMix in _musicMixes) musicMix.Stop();
        }

        public void FadeOutMusic(float fadeOutTime = 1f)
        {
            if (_musicMixes == null || _musicMixes.Count == 0) return;
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            _nextTrackTimer.Stop();
            _musicMixes[_currentMixIndex].FadeOut(fadeOutTime);
        }

        void PlayNextTrack()
        {
            if (!MusicEnabled) return;
            var clip = _currentMusicGroup?.GetNextMusicClip();
            if (!clip) return;
            ToggleCurrentMix();
            _musicMixes[_currentMixIndex].PlayClip(clip);
            StartNextTrackTimer(clip);
        }

        void ToggleCurrentMix()
        {
            _currentMixIndex = _currentMixIndex == 0 ? 1 : 0;
        }

        MusicClipGroup GetMusicGroup(string musicGroupName)
        {
            return _musicClipGroups?.FirstOrDefault(group => group.name == musicGroupName);
        }

        void InitializeMusicMixes()
        {
            AudioSettings.Instance.LoadAudioSettings();
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

        void CreateNextTrackTimer()
        {
            _nextTrackTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        }

        void StartNextTrackTimer(AudioClip clip)
        {
            _nextTrackTimer.OnTimerStop -= PlayNextTrack; 
            _nextTrackTimer.OnTimerStop += PlayNextTrack;
            _nextTrackTimer.Start(clip.length - 10f);
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

        void OnPlayMusicEvent(PlayMusicEvent playMusicEvent)
        {
            PlayMusic(playMusicEvent.MusicGroupName);
        }

        public void EnableMusic(bool musicEnabled = true)
        {
            MusicEnabled = musicEnabled;
            if (!MusicEnabled)
            {
                StopAllMusic();
                return;
            }
            PlayNextTrack();
        }
    }
}