using MidniteOilSoftware.Core;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    [SerializeField] AudioClip _alarmSound;
    
    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        EventBus.Instance.Subscribe<AlarmEvent>(OnAlarmEvent);
    }

    void OnAlarmEvent(AlarmEvent _)
    {
        _audioSource.PlayOneShot(_alarmSound);
    }
}

public struct AlarmEvent
{
}
