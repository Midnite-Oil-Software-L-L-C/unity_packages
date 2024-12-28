using MidniteOilSoftware.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] Button _timerButton;
    [SerializeField] TMP_Text _buttonText, _timerText;
    
    Timer _timer;
    
    void Start()
    {
        _timerText.gameObject.SetActive(false);
        _timer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _timer.OnTimerStop += TimerFinished;
        _buttonText.text = "Start Timer";
        _timerButton.onClick.AddListener(TimerButtonClicked);
    }

    void OnDisable()
    {
        _timer.OnTimerStop -= TimerFinished;
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_timer);
        _timerButton.onClick.RemoveListener(TimerButtonClicked);
    }

    void Update()
    {
        if (!_timer.IsRunning) return;
        _timerText.text = $"{_timer.Time:0.0}";
    }

    void TimerFinished()
    {
        EventBus.Instance.Raise(new AlarmEvent());
        _buttonText.text = "Start Timer";
        _timerText.gameObject.SetActive(false);   
    }

    void TimerButtonClicked()
    {
        if (_timer.IsRunning)
        {
            _timer.Stop(false);
            UpdateTimerButton();
        }
        else
        {
            _timer.Start(10f);
            UpdateTimerButton();
        }
    }

    void UpdateTimerButton()
    {
        _buttonText.text = _timer.IsRunning ? "Stop Timer" : "Start Timer";
        _timerText.gameObject.SetActive(_timer.IsRunning);
    }
}
