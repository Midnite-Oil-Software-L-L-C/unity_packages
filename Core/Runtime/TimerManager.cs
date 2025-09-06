using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MidniteOilSoftware.Core
{
    public class TimerManager : SingletonMonoBehaviour<TimerManager>
    {
        readonly Dictionary<Type, ObjectPool<Timer>> _pools = new();
        readonly List<Timer> _timers = new();
        
        public Timer CreateTimer<T>(float value = 0f)
        {
            if (_enableDebugLog)
            {
                Debug.Log($"Creating timer of type {typeof(T).Name} with initial value: {value}", this);
            }
            var pool = GetTimerPool<T>();
            var timer = pool.Get();
            timer.SetInitialTime(value);
            _timers.Add(timer);
            return timer;
        }

        public void ReleaseTimer<T>(Timer timer)
        {
            if (_enableDebugLog)
            {
                Debug.Log($"Releasing timer of type {typeof(T).Name}. IsRunning={timer?.IsRunning}", this);
            }
            if (timer == null) return;
            if (!_timers.Contains(timer)) return;
            timer.Stop(false);
            _timers.Remove(timer);
            var pool = GetTimerPool<T>();
            try
            {
                pool.Release(timer);
            }
            catch (Exception e)
            {
                if (_enableDebugLog)
                {
                    Debug.LogError($"Failed to release timer of type {typeof(T).Name}: {e.Message}", this);
                }
            }
        }

        IObjectPool<Timer> GetTimerPool<T>()
        {
            var type = typeof(T);
            if (!_pools.ContainsKey(type))
                _pools[type] = new ObjectPool<Timer>(() =>
                {
                    if (type == typeof(CountdownTimer))
                        return new CountdownTimer();
                    return type == typeof(StopwatchTimer) ? new StopwatchTimer() : null;
                });

            return _pools[type];
        }

        void Update()
        {
            for (var i = _timers.Count - 1; i >= 0; i--)
            {
                if (i < _timers.Count)
                    _timers[i].Tick(Time.deltaTime);
            }
        }
        
        protected override void OnRuntimeInitialize()
        {
            base.OnRuntimeInitialize();
    
            // TimerManager-specific initialization logic
            if (_enableDebugLog)
            {
                Debug.Log("TimerManager runtime initialization complete", this);
            }
    
            _pools.Clear();
            _timers.Clear();            
        }
    }
}