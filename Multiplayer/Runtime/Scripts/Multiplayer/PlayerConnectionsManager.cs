using System.Collections.Generic;
using System.Threading.Tasks;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Authentication;
using MidniteOilSoftware.Multiplayer.Events;
using MidniteOilSoftware.Multiplayer.Lobby;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class PlayerConnectionsManager : SingletonNetworkBehavior<PlayerConnectionsManager>
    {
        public readonly Dictionary<ulong, string> PlayerConnectionsToNames = new();
        public readonly Dictionary<ulong, string> PlayerConnectionsToIds = new();
        public readonly List<string> PlayerNames = new();

        public string GetPlayerId(ulong connectionId) =>
            PlayerConnectionsToIds.GetValueOrDefault(connectionId, "Unknown");

        public string GetName(ulong connectionId) =>
            PlayerConnectionsToNames.GetValueOrDefault(connectionId, "Unknown");

        readonly Dictionary<string, NetworkPlayer> _playerConnections = new();

        void Start()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = HandleConnectionApprovalOnServer;
        }

        public async Awaitable StartHostOnServer()
        {
            await AddPayloadData();
            NetworkManager.Singleton.StartHost();
        }
        
        public async Awaitable StopHostOnServer()
        {
            await Awaitable.WaitForSecondsAsync(1f);
            NetworkManager.Singleton.Shutdown();
        }

        async Task AddPayloadData()
        {
#if UNITY_EDITOR // Editor only Logging
            var profileName = FindFirstObjectByType<MultiplayerPlayModeManager>().ProfileName;
            Debug.Log($"PlayerConnectionsManager.AddPayloadData(): Getting PlayerName for Profile {profileName}");
#endif

            var playerName = AuthenticationManager.Instance.PlayerName;
            var playerId = AuthenticationService.Instance.PlayerId;
            await Awaitable.WaitForSecondsAsync(1f);

            ConnectionData data = new()
            {
                Name = playerName,
                Id = playerId
            };
            var json = JsonUtility.ToJson(data);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(json);
        }

        void HandleConnectionApprovalOnServer(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            Debug.Log($"Payload: {payload}");
            var data = JsonUtility.FromJson<ConnectionData>(payload);

            PlayerConnectionsToNames[request.ClientNetworkId] = data.Name;
            PlayerConnectionsToIds[request.ClientNetworkId] = data.Id;
            PlayerNames.Add(data.Name);

            response.Approved = true;

            _playerConnections.TryGetValue(data.Name, out var existingPlayer);
            if (existingPlayer != null)
            {
                existingPlayer.GetComponent<NetworkObject>().Despawn();
                _playerConnections.Remove(data.Name);
            }

            response.CreatePlayerObject = true;
        }

        public async Awaitable StartClient()
        {
            Debug.Log("Starting client");
            await AddPayloadData();
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.StartClient();
        }

        void OnClientStarted()
        {
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            Debug.Log("Client Started");
        }

        public void RegisterPlayerAndRemoveDuplicates(NetworkPlayer networkPlayer)
        {
            if (_playerConnections.TryGetValue(networkPlayer.PlayerName.Value.Value, out var existingPlayer))
            {
                existingPlayer.GetComponent<NetworkObject>().Despawn();
                _playerConnections.Remove(networkPlayer.PlayerName.Value.Value);
                Debug.LogWarning("3.5 Removed Duplicate Player for " + networkPlayer.PlayerName.Value.Value +
                                 " on Server");
            }

            _playerConnections.Add(networkPlayer.PlayerName.Value.Value, networkPlayer);
        }
    }
}