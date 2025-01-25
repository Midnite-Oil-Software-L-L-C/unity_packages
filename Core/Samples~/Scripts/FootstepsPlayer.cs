using System;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Core.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepsPlayer : MonoBehaviour
{
    [SerializeField] AudioEvent _footsteps;
    AudioSource _audioSource;

    void Start()
    {
        EventBus.Instance.Subscribe<FootstepsEvent>(OnFootstepsEvent);
        _audioSource = GetComponent<AudioSource>();
    }

    void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<FootstepsEvent>(OnFootstepsEvent);
    }

    void OnFootstepsEvent(FootstepsEvent _)
    {
        _footsteps.Play(_audioSource);
    }

}

public struct FootstepsEvent
{
}
