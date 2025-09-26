using System.Collections.Generic;
using System.Linq;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Authentication;
using MidniteOilSoftware.Multiplayer.Lobby;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class PlayerConnectionsManager : SingletonNetworkBehavior<PlayerConnectionsManager>
    {
        private readonly Dictionary<ulong, string> _playerConnectionsToNames = new();
        private readonly Dictionary<ulong, string> _playerConnectionsToIds = new();
        private readonly Dictionary<string, NetworkPlayer> _playerConnections = new();

        public string GetPlayerId(ulong connectionId) => _playerConnectionsToIds.GetValueOrDefault(connectionId, "Unknown");
        public string GetName(ulong connectionId) => _playerConnectionsToNames.GetValueOrDefault(connectionId, "Unknown");

        protected override void Start()
        {
            base.Start();
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = HandleConnectionApprovalOnServer;
        }
        
        void HandleConnectionApprovalOnServer(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = request.Payload;
            ConnectionData data;

            // Check if the payload is null or empty, which is the case for the host
            if (payload == null || payload.Length == 0)
            {
                // For the host, create a ConnectionData object from the local player's info
                data = new ConnectionData
                {
                    Name = AuthenticationManager.Instance.PlayerName,
                    Id = AuthenticationService.Instance.PlayerId
                };
                if (_enableDebugLog) Logwin.Log("PlayerConnectionsManager", "Payload is null. Assuming this is the host and creating ConnectionData.", "Multiplayer");
            }
            else
            {
                // For a connecting client, process the payload as usual
                var payloadString = System.Text.Encoding.UTF8.GetString(payload);
                if (_enableDebugLog) Logwin.Log("PlayerConnectionsManager", $"Payload: {payloadString}", "Multiplayer");
                data = JsonUtility.FromJson<ConnectionData>(payloadString);
            }

            _playerConnectionsToNames[request.ClientNetworkId] = data.Name;
            _playerConnectionsToIds[request.ClientNetworkId] = data.Id;

            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        public void RegisterPlayerAndRemoveDuplicates(NetworkPlayer networkPlayer)
        {
            if (!IsServer)
            {
                if (_enableDebugLog) 
                    Logwin.LogError("PlayerConnectionsManager", 
                        "RegisterPlayerAndRemoveDuplicates called on client, this should only be called on the server.", 
                        "Multiplayer");
                return;
            }
            if (_enableDebugLog) 
                Logwin.Log("PlayerConnectionsManager", 
                    $"Registering player {networkPlayer.PlayerName.Value.Value} with ClientId {networkPlayer.OwnerClientId} on Server", 
                    "Multiplayer");
            
            var duplicatePlayer = _playerConnections.Values.FirstOrDefault(p => p.OwnerClientId == networkPlayer.OwnerClientId);
            if (duplicatePlayer)
            {
                duplicatePlayer.GetComponent<NetworkObject>().Despawn();
                _playerConnections.Remove(duplicatePlayer.PlayerName.Value.Value);
                if (_enableDebugLog) 
                    Logwin.LogWarning("PlayerConnectionsManager", 
                        $"Removed Duplicate Player for ClientId {networkPlayer.OwnerClientId} on Server", 
                        "Multiplayer");
            }

            _playerConnections.Add(networkPlayer.PlayerName.Value.Value, networkPlayer);
        }

    }
}