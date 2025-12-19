using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    /// <summary>
    /// Generic singleton pattern for NetworkBehaviour components.
    /// All instances should be placed in the Bootstrapper scene as NetworkObjects.
    /// </summary>
    public class SingletonNetworkBehavior<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        [SerializeField] protected bool _enableDebugLog;
        
        private static T _instance;
        private static bool _isApplicationQuitting;

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    return null;
                }

                if (_instance) return _instance;

                _instance = FindFirstObjectByType<T>();

                return _instance ? _instance : CreateSingletonInstance();
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

        public override void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            base.OnDestroy();
        }

        protected virtual void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"SingletonNetworkBehavior:Multiplayer-[SingletonMonoBehaviour] Another instance of '{typeof(T)}' exists. Destroying: {gameObject.name}");
                Destroy(gameObject);
            }
        }
        
        protected virtual void Start()
        {
            if (_enableDebugLog)
            {
                Debug.Log($"SingletonNetworkBehavior:Multiplayer-[{typeof(T)}] Singleton instance started.");
            }
        }
    }
}