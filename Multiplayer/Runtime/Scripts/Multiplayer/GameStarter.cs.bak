using System.Collections;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private bool _autoStartWhenFull = true;
        [SerializeField] private int _maxPlayers = 2; // Set to 2 for Othello
        [SerializeField] bool _enableDebugLog = true; // Enable debug for troubleshooting

        IEnumerator Start()
        {
            if (!NetworkManager.Singleton.IsHost) yield break;
            
            // Subscribe to new connections
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            
            yield return HelperFunctions.GetWaitForSeconds(0.5f); // Wait a moment for players to register
            
            // Check if we already have enough players registered when the scene loads
            CheckForGameStart();
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            }
        }
        
        private void HandleClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsHost) return;
            CheckForGameStart();
        }
        
        private void CheckForGameStart()
        {
            var connectedClientsCount = NetworkManager.Singleton.ConnectedClients.Count;
            var registeredPlayersCount = PlayerRegistry.Instance?.PlayerCount ?? 0;
            
            if (_enableDebugLog)
            {
                Logwin.Log("GameStarter", 
                    $"Checking for game start. Connected clients: {connectedClientsCount}, PlayerRegistry players: {registeredPlayersCount}/{_maxPlayers}", 
                    "Multiplayer");
            }

            // Check both connected clients AND registered players to be safe
            if (!_autoStartWhenFull || connectedClientsCount < _maxPlayers || registeredPlayersCount < _maxPlayers) 
            {
                if (_enableDebugLog && connectedClientsCount >= _maxPlayers && registeredPlayersCount < _maxPlayers)
                {
                    Logwin.Log("GameStarter", 
                        "Connected clients ready but PlayerRegistry still registering players. Will retry...", 
                        "Multiplayer");
                    
                    // If clients are connected but not all registered yet, try again in a moment
                    StartCoroutine(DelayedCheckForGameStart());
                }
                return;
            }
            
            if (_enableDebugLog)
            {
                Logwin.Log("GameStarter", 
                    "Max players reached in both NetworkManager and PlayerRegistry, starting game.", 
                    "Multiplayer");
            }
            StartGame();
        }

        private IEnumerator DelayedCheckForGameStart()
        {
            yield return HelperFunctions.GetWaitForSeconds(0.3f);
            CheckForGameStart();
        }

        public void StartGame()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            
            if (_enableDebugLog)
            {
                Logwin.Log("GameStarter", "Raising GameStartedEvent", "Multiplayer");
            }
            
            EventBus.Instance.Raise<GameStartedEvent>(new GameStartedEvent());
        }
    }
}
