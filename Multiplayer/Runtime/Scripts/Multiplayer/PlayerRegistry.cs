using System.Collections.Generic;
using MidniteOilSoftware.Core;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class PlayerRegistry : SingletonNetworkBehavior<PlayerRegistry>
    {
        readonly Dictionary<ulong, NetworkPlayer> _registeredPlayers = new();
        readonly List<NetworkPlayer> _playerList = new();
        
        public IReadOnlyList<NetworkPlayer> Players => _playerList.AsReadOnly();
        public NetworkPlayer LocalPlayer { get; private set; }
        public int PlayerCount => _playerList.Count;
        
        public event System.Action<NetworkPlayer> OnPlayerRegistered;
        public event System.Action<NetworkPlayer> OnPlayerUnregistered;
        public event System.Action OnPlayersChanged;

        protected override void Awake()
        {
            base.Awake();
            if (_enableDebugLog)
                Debug.Log("Multiplayer:PlayerRegistry - PlayerRegistry Awake called - should persist across scenes");
        }

        protected override void Start()
        {
            base.Start();
            
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - PlayerRegistry Start - IsHost: {IsHost}, Current player count: {PlayerCount}");

            if (!IsHost) return;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        public override void OnDestroy()
        {
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - PlayerRegistry OnDestroy called");
                
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
            base.OnDestroy();
        }

        void HandleClientConnected(ulong clientId)
        {
            if (!IsHost) return;
            
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - Client {clientId} connected. Current player count: {PlayerCount}");
        }

        void HandleClientDisconnected(ulong clientId)
        {
            if (!IsHost) return;
            
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - Client {clientId} disconnected. Current player count: {PlayerCount}");
            
            if (_registeredPlayers.TryGetValue(clientId, out var player))
            {
                UnregisterPlayer(player);
            }
        }

        public void RegisterPlayer(NetworkPlayer player)
        {
            if (player == null)
            {
                if (_enableDebugLog)
                    Debug.LogError($"Multiplayer:PlayerRegistry - Attempted to register null player");
                return;
            }

            var clientId = player.OwnerClientId;
            
            if (_registeredPlayers.ContainsKey(clientId))
            {
                if (_enableDebugLog)
                    Debug.LogWarning($"Multiplayer:PlayerRegistry - Player with clientId {clientId} already registered. Skipping.");
                return;
            }

            _registeredPlayers[clientId] = player;
            _playerList.Add(player);
            
            if (player.IsLocalPlayer)
            {
                LocalPlayer = player;
                if (_enableDebugLog)
                    Debug.Log($"Multiplayer:PlayerRegistry - Set LocalPlayer to {player.PlayerName.Value}");
            }

            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - ✅ Registered player {player.PlayerName.Value} (ClientId: {clientId}). Total players: {_playerList.Count}");       

            OnPlayerRegistered?.Invoke(player);
            OnPlayersChanged?.Invoke();
            
            // Sync to all clients
            if (IsHost)
            {
                NotifyPlayerRegisteredClientRpc(clientId);
            }
        }

        public void UnregisterPlayer(NetworkPlayer player)
        {
            if (player == null) return;

            var clientId = player.OwnerClientId;
            
            if (!_registeredPlayers.ContainsKey(clientId))
            {
                if (_enableDebugLog)
                    Debug.LogWarning($"Multiplayer:PlayerRegistry - Attempted to unregister player {clientId} that wasn't registered");
                return;
            }

            _registeredPlayers.Remove(clientId);
            _playerList.Remove(player);
            
            if (player == LocalPlayer)
            {
                LocalPlayer = null;
                if (_enableDebugLog)
                    Debug.Log($"Multiplayer:PlayerRegistry - LocalPlayer set to null");
            }

            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - Unregistered player {player.PlayerName.Value} (ClientId: {clientId}). Total players: {_playerList.Count}");

            OnPlayerUnregistered?.Invoke(player);
            OnPlayersChanged?.Invoke();
            
            // Sync to all clients
            if (IsHost)
            {
                NotifyPlayerUnregisteredClientRpc(clientId);
            }
        }

        public NetworkPlayer GetPlayer(ulong clientId)
        {
            return _registeredPlayers.GetValueOrDefault(clientId);
        }

        public NetworkPlayer GetPlayerByIndex(int index)
        {
            return index >= 0 && index < _playerList.Count ? _playerList[index] : null;
        }

        public int GetPlayerIndex(NetworkPlayer player)
        {
            return _playerList.IndexOf(player);
        }

        public List<NetworkPlayer> GetPlayers()
        {
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - GetPlayers called - returning {_playerList.Count} players");
            return new List<NetworkPlayer>(_playerList);
        }

        public void ClearPlayers()
        {
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - Clearing all players");
            
            _registeredPlayers.Clear();
            _playerList.Clear();
            LocalPlayer = null;
            OnPlayersChanged?.Invoke();
        }

        [Rpc(SendTo.NotServer)]
        void NotifyPlayerRegisteredClientRpc(ulong clientId)
        {
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - NotifyPlayerRegisteredClientRpc for client {clientId}");
                
            // On clients, we need to find the player object and register it locally
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObject != null)
            {
                var player = playerObject.GetComponent<NetworkPlayer>();
                if (player != null)
                {
                    RegisterPlayerLocal(player);
                }
                else
                {
                    if (_enableDebugLog)
                        Debug.LogError($"Multiplayer:PlayerRegistry - No NetworkPlayer component on player object for client {clientId}");
                }
            }
            else
            {
                if (_enableDebugLog)
                    Debug.LogError($"Multiplayer:PlayerRegistry - No player object found for client {clientId}");
            }
        }

        [Rpc(SendTo.NotServer)]
        void NotifyPlayerUnregisteredClientRpc(ulong clientId)
        {
            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - NotifyPlayerUnregisteredClientRpc for client {clientId}");
                
            if (_registeredPlayers.TryGetValue(clientId, out var player))
            {
                UnregisterPlayerLocal(player);
            }
        }

        void RegisterPlayerLocal(NetworkPlayer player)
        {
            var clientId = player.OwnerClientId;
            
            if (_registeredPlayers.ContainsKey(clientId))
            {
                if (_enableDebugLog)
                    Debug.LogWarning($"Multiplayer:PlayerRegistry - Client: Player {clientId} already registered locally");
                return;
            }

            _registeredPlayers[clientId] = player;
            _playerList.Add(player);
            
            if (player.IsLocalPlayer)
            {
                LocalPlayer = player;
            }

            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - Client: Registered player {player.PlayerName.Value} locally. Total: {_playerList.Count}");

            OnPlayerRegistered?.Invoke(player);
            OnPlayersChanged?.Invoke();
        }

        void UnregisterPlayerLocal(NetworkPlayer player)
        {
            var clientId = player.OwnerClientId;
            
            if (!_registeredPlayers.ContainsKey(clientId)) return;

            _registeredPlayers.Remove(clientId);
            _playerList.Remove(player);
            
            if (player == LocalPlayer)
            {
                LocalPlayer = null;
            }

            if (_enableDebugLog)
                Debug.Log($"Multiplayer:PlayerRegistry - Client: Unregistered player {player.PlayerName.Value} locally. Total: {_playerList.Count}");

            OnPlayerUnregistered?.Invoke(player);
            OnPlayersChanged?.Invoke();
        }

        // Add a debug method to check current state
        [ContextMenu("Debug Player State")]
        public void DebugPlayerState()
        {
            Debug.Log("Multiplayer:PlayerRegistry - === PLAYER REGISTRY DEBUG ===");
            Debug.Log($"Multiplayer:PlayerRegistry - Total registered players: {PlayerCount}");
            Debug.Log($"Multiplayer:PlayerRegistry - LocalPlayer: {(LocalPlayer ? LocalPlayer.PlayerName.Value.ToString() : "null")}");

            for (int i = 0; i < _playerList.Count; i++)
            {
                var player = _playerList[i];
                Debug.Log($"Multiplayer:PlayerRegistry - PlayerRegistry - Player {i}: {player.PlayerName.Value} (ClientId: {player.OwnerClientId}, IsLocal: {player.IsLocalPlayer})");
            }
            Debug.Log("Multiplayer:PlayerRegistry - PlayerRegistry - === END DEBUG ===");
        }
    }
}
