using System;

namespace MidniteOilSoftware.Core
{
    public abstract class Timer
    {
        public float InitialTime { get; protected set; }
        public float Time { get; protected set; }
        public bool IsRunning { get; private set; }
        public float Progress => Time / InitialTime;

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };
        bool _debugMode;

        protected Timer()
        {
            IsRunning = false;
#if !UNITY_EDITOR
            _debugMode = false;
#endif
        }

        public void SetInitialTime(float value, bool debugMode = false)
        {
            _debugMode = debugMode;
            if (_debugMode)
            {
                Debug.Log($"Setting initial time to {value}. Previous InitialTime={InitialTime}");
                if (value < 0)
                {
                    Debug.LogError("Initial time cannot be negative. Setting to 0.");
                    value = 0;
                }

            }

            InitialTime = value;
        }

        public void Start(float? initialTime = null)
        {
            if (initialTime != null) InitialTime = (float)initialTime;
            Time = InitialTime;
            if (_debugMode)
            {
                Debug.Log($"Starting timer with initial time: {InitialTime}. IsRunning={IsRunning}");
            }
            if (IsRunning) return;
            IsRunning = true;
            OnTimerStart.Invoke();
        }

        public void Stop(bool invokeStopEvent = true)
        {
            if (_debugMode)
            {
                Debug.Log($"Stopping timer. IsRunning={IsRunning}. Time remaining={Time}. invokeStopEvent={invokeStopEvent}");
            }
            if (!IsRunning) return;
            IsRunning = false;
            if (invokeStopEvent) OnTimerStop.Invoke();
        }

        public void Resume()
        {
            IsRunning = true;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public abstract void Tick(float deltaTime);
    }

    public class CountdownTimer : Timer
    {
        public CountdownTimer() : base()
        {
        }

        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0) Time -= deltaTime;

            if (IsRunning && Time <= 0) Stop();
        }

        public bool IsFinished => Time <= 0;

        public void Reset()
        {
            Time = InitialTime;
        }

        public void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }
    }

    public class StopwatchTimer : Timer
    {
        public override void Tick(float deltaTime)
        {
            if (IsRunning) Time += deltaTime;
        }

        public void Reset()
        {
            Time = 0;
        }

        public float GetTime()
        {
            return Time;
        }
    }
}