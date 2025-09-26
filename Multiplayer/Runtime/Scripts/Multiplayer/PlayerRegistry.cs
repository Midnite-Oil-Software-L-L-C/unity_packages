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
                Logwin.Log("PlayerRegistry", "PlayerRegistry Awake called - should persist across scenes", "Multiplayer");
        }

        protected override void Start()
        {
            base.Start();
            
            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", $"PlayerRegistry Start - IsHost: {IsHost}, Current player count: {PlayerCount}", "Multiplayer");

            if (!IsHost) return;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        public override void OnDestroy()
        {
            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", "PlayerRegistry OnDestroy called", "Multiplayer");
                
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
                Logwin.Log("PlayerRegistry", $"Client {clientId} connected. Current player count: {PlayerCount}", "Multiplayer");
        }

        void HandleClientDisconnected(ulong clientId)
        {
            if (!IsHost) return;
            
            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", $"Client {clientId} disconnected. Current player count: {PlayerCount}", "Multiplayer");
            
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
                    Logwin.LogError("PlayerRegistry", "Attempted to register null player", "Multiplayer");
                return;
            }

            var clientId = player.OwnerClientId;
            
            if (_registeredPlayers.ContainsKey(clientId))
            {
                if (_enableDebugLog)
                    Logwin.LogWarning("PlayerRegistry", 
                        $"Player with clientId {clientId} already registered. Skipping.", "Multiplayer");
                return;
            }

            _registeredPlayers[clientId] = player;
            _playerList.Add(player);
            
            if (player.IsLocalPlayer)
            {
                LocalPlayer = player;
                if (_enableDebugLog)
                    Logwin.Log("PlayerRegistry", $"Set LocalPlayer to {player.PlayerName.Value}", "Multiplayer");
            }

            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", 
                    $"✅ Registered player {player.PlayerName.Value} (ClientId: {clientId}). Total players: {_playerList.Count}", 
                    "Multiplayer");

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
                    Logwin.LogWarning("PlayerRegistry", 
                        $"Attempted to unregister player {clientId} that wasn't registered", "Multiplayer");
                return;
            }

            _registeredPlayers.Remove(clientId);
            _playerList.Remove(player);
            
            if (player == LocalPlayer)
            {
                LocalPlayer = null;
                if (_enableDebugLog)
                    Logwin.Log("PlayerRegistry", "LocalPlayer set to null", "Multiplayer");
            }

            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", 
                    $"❌ Unregistered player {player.PlayerName.Value} (ClientId: {clientId}). Total players: {_playerList.Count}", 
                    "Multiplayer");

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
                Logwin.Log("PlayerRegistry", $"GetPlayers called - returning {_playerList.Count} players", "Multiplayer");
            return new List<NetworkPlayer>(_playerList);
        }

        public void ClearPlayers()
        {
            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", "Clearing all players", "Multiplayer");
            
            _registeredPlayers.Clear();
            _playerList.Clear();
            LocalPlayer = null;
            OnPlayersChanged?.Invoke();
        }

        [Rpc(SendTo.NotServer)]
        void NotifyPlayerRegisteredClientRpc(ulong clientId)
        {
            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", $"NotifyPlayerRegisteredClientRpc for client {clientId}", "Multiplayer");
                
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
                        Logwin.LogError("PlayerRegistry", $"No NetworkPlayer component on player object for client {clientId}", "Multiplayer");
                }
            }
            else
            {
                if (_enableDebugLog)
                    Logwin.LogError("PlayerRegistry", $"No player object found for client {clientId}", "Multiplayer");
            }
        }

        [Rpc(SendTo.NotServer)]
        void NotifyPlayerUnregisteredClientRpc(ulong clientId)
        {
            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", $"NotifyPlayerUnregisteredClientRpc for client {clientId}", "Multiplayer");
                
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
                    Logwin.LogWarning("PlayerRegistry", 
                        $"Client: Player {clientId} already registered locally", "Multiplayer");
                return;
            }

            _registeredPlayers[clientId] = player;
            _playerList.Add(player);
            
            if (player.IsLocalPlayer)
            {
                LocalPlayer = player;
            }

            if (_enableDebugLog)
                Logwin.Log("PlayerRegistry", 
                    $"Client: Registered player {player.PlayerName.Value} locally. Total: {_playerList.Count}", 
                    "Multiplayer");

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
                Logwin.Log("PlayerRegistry", 
                    $"Client: Unregistered player {player.PlayerName.Value} locally. Total: {_playerList.Count}", 
                    "Multiplayer");

            OnPlayerUnregistered?.Invoke(player);
            OnPlayersChanged?.Invoke();
        }

        // Add a debug method to check current state
        [ContextMenu("Debug Player State")]
        public void DebugPlayerState()
        {
            Logwin.Log("PlayerRegistry", "=== PLAYER REGISTRY DEBUG ===", "Multiplayer");
            Logwin.Log("PlayerRegistry", $"Total registered players: {PlayerCount}", "Multiplayer");
            Logwin.Log("PlayerRegistry", $"LocalPlayer: {(LocalPlayer ? LocalPlayer.PlayerName.Value.ToString() : "null")}", "Multiplayer");
            
            for (int i = 0; i < _playerList.Count; i++)
            {
                var player = _playerList[i];
                Logwin.Log("PlayerRegistry", 
                    $"Player {i}: {player.PlayerName.Value} (ClientId: {player.OwnerClientId}, IsLocal: {player.IsLocalPlayer})", 
                    "Multiplayer");
            }
            Logwin.Log("PlayerRegistry", "=== END DEBUG ===", "Multiplayer");
        }
    }
}
