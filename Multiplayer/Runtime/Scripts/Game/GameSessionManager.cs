using System.Collections;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameSessionManager : SingletonMonoBehaviour<GameSessionManager>
    {
        [SerializeField] GameSessionInitializer _gameSessionInitializer;
        [SerializeField] GameSessionCleanup _gameSessionCleanup;

        void Start()
        {
            NetworkManager.Singleton.OnServerStarted += InitializeSession;
            NetworkManager.Singleton.OnServerStarted += InitializeSession;
        }

        void InitializeSession()
        {
            _gameSessionInitializer.InitializeSession();
        }
        
        public void CleanupSession()
        {
            StartCoroutine(CleanupSessionAsync());
        }

        IEnumerator CleanupSessionAsync()
        {
            yield return _gameSessionCleanup.CleanupSession();
            yield return PlayerConnectionsManager.Instance.StopHostOnServer();
            EventBus.Instance.Raise<LeftGameEvent>(new LeftGameEvent());
        }
    }
}
