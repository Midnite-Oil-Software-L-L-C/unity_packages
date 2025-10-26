using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Authentication;

namespace MidniteOilSoftware.Multiplayer
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] bool _enableDebugLog = true;
        
        public ulong ConnectionId => OwnerClientId;
        public NetworkVariable<FixedString32Bytes> PlayerId;
        public NetworkVariable<FixedString32Bytes> PlayerName;

        GameManager _gameManager;

        GameManager GameManager
        {
            get
            {
                if (!_gameManager) _gameManager = FindFirstObjectByType<GameManager>();
                return _gameManager;
            }
        }

        void Awake()
        {
            PlayerId = new();
            PlayerName = new();
            DontDestroyOnLoad(gameObject);
            
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", $"NetworkPlayer Awake - DontDestroyOnLoad set", "Multiplayer");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
    
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"NetworkPlayer OnNetworkSpawn - ClientId: {OwnerClientId}, IsLocalPlayer: {IsLocalPlayer}", 
                    "Multiplayer");
    
            // Send player name to server if this is the local player
            if (IsLocalPlayer && !IsHost)
            {
                var playerName = AuthenticationManager.Instance?.PlayerName ?? "Unknown";
                if (_enableDebugLog)
                    Logwin.Log("NetworkPlayer", 
                        $"Sending player name '{playerName}' to server", 
                        "Multiplayer");
                SetPlayerNameServerRpc(playerName);
            }
    
            // Initialize player data (only server can set NetworkVariables)
            InitializePlayer();
    
            // Register with PlayerRegistry once spawned
            if (!PlayerRegistry.Instance) return;
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"Registering player {OwnerClientId} with PlayerRegistry", "Multiplayer");
            PlayerRegistry.Instance.RegisterPlayer(this);
        }

        public override void OnNetworkDespawn()
        {
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"NetworkPlayer OnNetworkDespawn - ClientId: {OwnerClientId}", "Multiplayer");
            
            // Unregister from PlayerRegistry when despawning
            if (PlayerRegistry.Instance != null)
            {
                if (_enableDebugLog)
                    Logwin.Log("NetworkPlayer", 
                        $"Unregistering player {OwnerClientId} from PlayerRegistry", "Multiplayer");
                PlayerRegistry.Instance.UnregisterPlayer(this);
            }
            
            base.OnNetworkDespawn();
        }

        void InitializePlayer()
        {
            if (!IsServer) return; // only server can set NetworkVariables!
    
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"InitializePlayer called - ClientId: {OwnerClientId}, IsServer: {IsServer}", 
                    "Multiplayer");
    
            // For host player, set name immediately
            if (IsLocalPlayer)
            {
                var playerName = AuthenticationManager.Instance?.PlayerName ?? "Unknown";
                PlayerName.Value = playerName;
                gameObject.name = "NetworkPlayer (" + PlayerName.Value + ")";
        
                if (_enableDebugLog)
                    Logwin.Log("NetworkPlayer", 
                        $"Set host player name to '{playerName}'", 
                        "Multiplayer");
            }
            // For remote players, the name will be set via SetPlayerNameServerRpc
    
            var playerId = PlayerConnectionsManager.Instance.GetPlayerId(OwnerClientId);
            PlayerId.Value = playerId;
    
            PlayerConnectionsManager.Instance.RegisterPlayerAndRemoveDuplicates(this);

            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"Player initialization completed for {OwnerClientId}: {PlayerName.Value}. PlayerId={PlayerId.Value}", 
                    "Multiplayer");
        }

        [ServerRpc(RequireOwnership = false)]
        void SetPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;
    
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"Received SetPlayerNameServerRpc from client {clientId} with name '{playerName}'", 
                    "Multiplayer");
    
            if (OwnerClientId != clientId) return;
            PlayerName.Value = playerName;
            gameObject.name = "NetworkPlayer (" + PlayerName.Value + ")";
        
            if (_enableDebugLog)
                Logwin.Log("NetworkPlayer", 
                    $"Set PlayerName to '{playerName}' for ClientId {clientId}", 
                    "Multiplayer");
        }
    }
}
