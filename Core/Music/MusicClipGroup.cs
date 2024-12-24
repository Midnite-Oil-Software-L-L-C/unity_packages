using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MidniteOilSoftware.Core.Music
{
    [CreateAssetMenu(fileName = "MusicClipGroup", menuName = "Midnite Oil Software/Music Clip Group")]
    public class MusicClipGroup : ScriptableObject
    {
        [SerializeField] string _groupName;
        [SerializeField] AudioClip[] _musicClips;

        readonly Queue<AudioClip> _trackQueue = new();

        public AudioClip GetNextMusicClip()
        {
            if (_trackQueue.Count == 0) ShuffleTrackQueue();
            var audioClip = _trackQueue.Dequeue();
            return audioClip;
        }

        void ShuffleTrackQueue()
        {
            if (_trackQueue.Count != 0) return;
            foreach (var audioClip in _musicClips.OrderBy(_ => Random.value))
            {
                _trackQueue.Enqueue(audioClip);
            }
        }
    }
}