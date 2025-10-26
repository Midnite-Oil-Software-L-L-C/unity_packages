using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace MidniteOilSoftware.Core
{
    /// <summary>
    /// A generic singleton MonoBehaviour class that can be inherited from to create a singleton MonoBehaviour.
    /// </summary>
    /// <typeparam name="T">The type of the singleton MonoBehaviour.</typeparam>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] bool _isPersistent = true;
        [FormerlySerializedAs("_debugMode")] [SerializeField] protected bool _enableDebugLog = false;

        static T _instance;
        static readonly object _lock = new();
        static bool _isApplicationQuitting;

        public static T Instance
        {
            get
            {
                // If the application is quitting, we should not create or return an instance
                if (_isApplicationQuitting) return null;

                lock (_lock)
                {
                    // If an instance already exists, return it
                    if (_instance) return _instance;

                    // Try to find an existing instance in the scene
                    _instance = FindFirstObjectByType<T>();

                    // If no instance found, create a new one
                    return _instance ? _instance : CreateSingletonInstance();
                }
            }
        }

        static T CreateSingletonInstance()
        {
            var singletonObject = new GameObject(typeof(T).Name);
            DontDestroyOnLoad(singletonObject);
            _instance = singletonObject.AddComponent<T>();
            return _instance;
        }

        // Using an Action to allow external classes to subscribe to application quitting event
        public static event Action OnApplicationQuittingEvent;

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
            OnApplicationQuittingEvent?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            // Only clear the instance if this is the singleton instance and the application is quitting
            if (_instance != this || !_isApplicationQuitting) return;
            _instance = null;
            // Unsubscribe from the static event to prevent memory leaks
            OnApplicationQuittingEvent -= OnApplicationQuittingInternal;
        }

        protected virtual void Awake()
        {
            #if !UNITY_EDITOR
            _enableDebugLog = false; // Disable debug logging in non-editor builds
            #endif
            var typeName = typeof(T).Name;
            var instanceId = gameObject.GetInstanceID();

            if (_enableDebugLog)
            {
                Logwin.Log("SingletonMonoBehaviour",
                    $"[{typeName}] Awake called on GameObject: {gameObject.name} (ID: {instanceId})", "Core");
            }

            // This is crucial for handling domain reload disabled
            if (_instance == null)
            {
                _instance = this as T;

                if (_enableDebugLog)
                {
                    Logwin.Log("SingletonMonoBehaviour",
                        $"[{typeName}] Setting as singleton instance: {gameObject.name}", "Core");
                }

                // Subscribe once to handle application quitting
                OnApplicationQuittingEvent -= OnApplicationQuittingInternal;
                OnApplicationQuittingEvent += OnApplicationQuittingInternal;

                // Enhanced debugging for DontDestroyOnLoad
                if (!_isPersistent) return;
                var sceneName = gameObject.scene.name;

                if (_enableDebugLog)
                {
                    Logwin.Log("SingletonMonoBehaviour",
                        $"[{typeName}] Current scene: '{sceneName}', attempting DontDestroyOnLoad", "Core");
                }

                if (sceneName != "DontDestroyOnLoad")
                {
                    // Add a check for existing DontDestroyOnLoad objects of the same type
                    var existingInstances =
                        FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    var dontDestroyInstances = 0;

                    foreach (var instance in existingInstances)
                    {
                        if (instance.gameObject.scene.name != "DontDestroyOnLoad") continue;
                        dontDestroyInstances++;
                        if (_enableDebugLog)
                        {
                            Logwin.LogWarning("SingletonMonoBehaviour",
                                $"[{typeName}] Found existing DontDestroyOnLoad instance: {instance.gameObject.name}",
                                "Core");
                        }
                    }

                    if (dontDestroyInstances > 0)
                    {
                        Logwin.LogError("SingletonMonoBehaviour",
                            $"[{typeName}] Cannot call DontDestroyOnLoad - {dontDestroyInstances} instances already exist in DontDestroyOnLoad scene!",
                            "Core");
                        return;
                    }

                    if (_enableDebugLog)
                    {
                        Logwin.Log("SingletonMonoBehaviour",
                            $"[{typeName}] Calling DontDestroyOnLoad on {gameObject.name}", "Core");
                    }
                    #if UNITY_EDITOR
                    if (Application.isPlaying)
                        UnityEditor.SceneVisibilityManager.instance.Show(gameObject, false);
                    #endif
                    DontDestroyOnLoad(gameObject);

                    if (_enableDebugLog)
                    {
                        Logwin.Log("SingletonMonoBehaviour",
                            $"[{typeName}] Successfully called DontDestroyOnLoad", "Core");
                    }
                }
                else if (_enableDebugLog)
                {
                    Logwin.Log("SingletonMonoBehaviour",
                        $"[{typeName}] GameObject already in DontDestroyOnLoad scene", "Core");
                }
            }
            else if (_instance != this)
            {
                if (_enableDebugLog)
                {
                    Logwin.LogWarning("SingletonMonoBehaviour",
                        $"[{typeName}] Another instance exists. Destroying: {gameObject.name}", "Core");
                }

                Destroy(gameObject);
            }
            else if (_enableDebugLog)
            {
                Logwin.Log("SingletonMonoBehaviour",
                    $"[{typeName}] This is the existing singleton instance", "Core");
            }
        }
        
        protected virtual void Start()
        {
            if (_enableDebugLog)
            {
                Logwin.Log("SingletonMonoBehaviour",
                    $"[{typeof(T)}] Singleton instance started.", "Core");
            }
        }

        // Internal method to handle application quitting, subscribed to the static event
        static void OnApplicationQuittingInternal()
        {
            _isApplicationQuitting = true;
            OnApplicationQuittingEvent -= OnApplicationQuittingInternal;
        }

        // REMOVED: StaticRuntimeInitialize method - this doesn't work with generic classes
        // The initialization is now handled by the SingletonInitializer class

        protected virtual void OnRuntimeInitialize()
        {
            // Base implementation - can be overridden by derived classes
            if (_enableDebugLog)
            {
                Debug.Log($"Runtime initializing {typeof(T).Name}", this);
            }
            // Resetting _isApplicationQuitting here is critical for domain reload disabled scenarios
            _isApplicationQuitting = false;
            // Ensure we are subscribed to the quitting event if not already
            OnApplicationQuittingEvent -= OnApplicationQuittingInternal; // Unsubscribe first to prevent double subscription
            OnApplicationQuittingEvent += OnApplicationQuittingInternal;
        }
    }
}
