using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class SingletonNetworkBehavior<T> : NetworkBehaviour where T : NetworkBehaviour
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

                    // Look for an existing instance
                    _instance = FindFirstObjectByType<T>();

                    return _instance != null ? _instance : CreateSingletonInstance();
                }
            }
        }

        static T CreateSingletonInstance()
        {
            Debug.Log($"Creating Singleton Instance of {typeof(T)}");
            var singletonObject = new GameObject(typeof(T).Name);
            _instance = singletonObject.AddComponent<T>();
            DontDestroyOnLoad(singletonObject);
            return _instance;
        }

        // Ensure that the singleton instance is reset on application quit
        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        // Reset the quitting flag when a new instance is created
        public override void OnDestroy()
        {
            if (_instance == this)
            {
                _isApplicationQuitting = true;
            }

            base.OnDestroy();
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