using MidniteOilSoftware.Core;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class SingletonNetworkBehavior<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        [SerializeField] protected bool _enableDebugLog;
        
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
                    if (_instance) return _instance;

                    // Look for an existing instance
                    _instance = FindFirstObjectByType<T>();

                    return _instance ? _instance : CreateSingletonInstance();
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Initialize()
        {
            _isApplicationQuitting = false;
            if (!_instance) return;
            Destroy(_instance);
            _instance = null;
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
            if (!_instance)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Logwin.LogWarning("SingletonNetworkBehavior",
                    $"[SingletonMonoBehaviour] Another instance of '{typeof(T)}' exists. Destroying: {gameObject.name}",
                    "Multiplayer");
                Destroy(gameObject);
            }
        }
        
        protected virtual void Start()
        {
            if (_enableDebugLog)
            {
                Logwin.Log("SingletonNetworkBehavior", $"[{typeof(T)}] Singleton instance started.", "Multiplayer");
            }
        }
    }
}