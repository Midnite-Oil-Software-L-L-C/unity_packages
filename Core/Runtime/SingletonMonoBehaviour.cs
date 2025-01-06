using UnityEngine;

namespace MidniteOilSoftware.Core
{
    /// <summary>
    /// A generic singleton MonoBehaviour class that can be inherited from to create a singleton MonoBehaviour.
    /// </summary>
    /// <typeparam name="T">The type of the singleton MonoBehaviour.</typeparam>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _isApplicationQuitting;

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (_instance != null) return _instance;

                    _instance = FindFirstObjectByType<T>();

                    return _instance != null ? _instance : CreateSingletonInstance();
                }
            }
        }

        static T CreateSingletonInstance()
        {
            var singletonObject = new GameObject(typeof(T).Name);
            _instance = singletonObject.AddComponent<T>();
            DontDestroyOnLoad(singletonObject);
            return _instance;
        }

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _isApplicationQuitting = true;
            }
        }

        // Optional: Override Awake in subclasses to add initialization logic
        protected virtual void Awake()
        {
            // Prevent multiple instances from existing
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning(
                    $"[SingletonMonoBehaviour] Another instance of '{typeof(T)}' exists. Destroying: {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }
}