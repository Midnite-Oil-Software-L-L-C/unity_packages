using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidniteOilSoftware.Core
{
    public class EventBus : SingletonMonoBehaviour<EventBus>
    {
        readonly Dictionary<Type, Delegate> _eventDictionary = new();

        protected override void OnDestroy()
        {
            if (_enableDebugLog) Debug.Log("Destroying EventBus instance and clearing event subscriptions", this);
            _eventDictionary.Clear();
            base.OnDestroy();
        }

        public void Subscribe<T>(Action<T> listener)
        {
            var eventType = typeof(T);

            if (!_eventDictionary.TryAdd(eventType, listener))
                _eventDictionary[eventType] = Delegate.Combine(_eventDictionary[eventType], listener);
        }

        public void Unsubscribe<T>(Action<T> listener)
        {
            var eventType = typeof(T);

            if (!_eventDictionary.TryGetValue(eventType, out var currentDelegate)) return;
            currentDelegate = Delegate.Remove(currentDelegate, listener);

            if (currentDelegate == null)
                _eventDictionary.Remove(eventType);
            else
                _eventDictionary[eventType] = currentDelegate;
        }

        public void Raise<T>(T eventArgs)
        {
            var eventType = typeof(T);
            if (!_eventDictionary.TryGetValue(eventType, out var value)) return;
            var currentDelegate = value as Action<T>;
            currentDelegate?.Invoke(eventArgs);
        }
        
        protected override void OnRuntimeInitialize()
        {
            base.OnRuntimeInitialize();
    
            if (_enableDebugLog)
            {
                Debug.Log("EventBus runtime initialization complete", this);
            }
    
            // Clear any existing event subscriptions on restart
            _eventDictionary.Clear();
        }

    }
}