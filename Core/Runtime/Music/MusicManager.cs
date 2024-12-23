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
        MusicClipGroup _currentMusicGroup;
        Timer _nextTrackTimer;

#if UNITY_EDITOR
        [ContextMenu("Play menu music")]
        public void PlayMenuMusic()
        {
            PlayMusic("Menu");
        }

        [ContextMenu("Play game music")]
        public void PlayGameMusic()
        {
            PlayMusic("Game");
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
#endif

        public void PlayMusic(string musicGroupName)
        {
            _currentMusicGroup = GetMusicGroup(musicGroupName);
            PlayNextTrack();
        }

        public void StopAllMusic()
        {
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            _nextTrackTimer.Stop();
            foreach (var musicMix in _musicMixes)
            {
                musicMix.Stop();
            }
        }

        void PlayNextTrack()
        {
            var clip = _currentMusicGroup.GetNextMusicClip();
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
            return _musicClipGroups.FirstOrDefault(group => group.name == musicGroupName);
        }

        void InitializeMusicMixes()
        {
            foreach (var musicMix in _musicMixes)
            {
                musicMix.Initialize();
            }
            AudioSettings.Instance.LoadAudioSettings();
        }

        void Start()
        {
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
            _nextTrackTimer.OnTimerStop += PlayNextTrack;
            _nextTrackTimer.Start(clip.length - 10f);
        }

        void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<PlayMusicEvent>(OnPlayMusicEvent);
            EventBus.Instance.Subscribe<StopAllMusicEvent>(_ => StopAllMusic());
        }

        void UnsubscribeFromEvents()
        {
            EventBus.Instance?.Unsubscribe<PlayMusicEvent>(OnPlayMusicEvent);
            EventBus.Instance?.Unsubscribe<StopAllMusicEvent>(_ => StopAllMusic());
        }

        void ReleaseNextTrackTimer()
        {
            _nextTrackTimer.OnTimerStop -= PlayNextTrack;
            TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_nextTrackTimer);
        }

        void OnPlayMusicEvent(PlayMusicEvent playMusicEvent)
        {
            PlayMusic(playMusicEvent.MusicGroupName);
        }
    }
}