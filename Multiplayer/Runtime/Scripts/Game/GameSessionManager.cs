using System.Collections;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameSessionManager : SingletonMonoBehaviour<GameSessionManager>
    {
        [SerializeField] GameSessionInitializer _gameSessionInitializer;
        [SerializeField] GameSessionCleanup _gameSessionCleanup;
        [SerializeField] float _cleanupDelay = 1.0f;
        
        public int MaxPlayers => _gameSessionInitializer ? _gameSessionInitializer.MaxPlayers : 4;
        
        protected override void Start()
        {
            base.Start();
            if (!_gameSessionInitializer)
            {
                _gameSessionInitializer = GetComponent<GameSessionInitializer>();
            }

            if (!_gameSessionCleanup)
            {
                _gameSessionCleanup = GetComponent<GameSessionCleanup>();
            }

            if (!_enableDebugLog) return;
            if (!_gameSessionInitializer)
            {
                Logwin.LogError("GameSessionManager", 
                    "No GameSessionInitializer found on GameSessionManager, please add one or override InitializeSession in a derived class.",
                    "Multiplayer");
            }
            if (!_gameSessionCleanup)
            {
                Logwin.LogError("GameSessionManager", 
                    "No GameSessionCleanup found on GameSessionManager, please add one or override CleanupSession in a derived class.",
                    "Multiplayer");
            }
        }
        public void InitializeSession()
        {
            if (_enableDebugLog)
                Logwin.Log("GameSessionManager", "Initializing game session...", "Multiplayer");
            _gameSessionInitializer?.InitializeSession();
            EventBus.Instance.Raise<GameSessionInitializedEvent>(new GameSessionInitializedEvent());
        }
        
        public void CleanupSession()
        {
            StartCoroutine(CleanupSessionAsync());
        }

        IEnumerator CleanupSessionAsync()
        {
            var cleanupCoroutine = StartCoroutine(_gameSessionCleanup?.CleanupSession());
            // Wait for the cleanup coroutine to finish if it exists
            if (cleanupCoroutine != null)
            {
                yield return cleanupCoroutine;
            }
            EventBus.Instance.Raise<GameSessionCleanupEvent>(new GameSessionCleanupEvent());
            
            // This now waits for any cleanup logic to finish before shutting down the host.
            yield return HelperFunctions.GetWaitForSeconds(_cleanupDelay); 
            EventBus.Instance.Raise<LeftGameEvent>(new LeftGameEvent());
        }
    }
}