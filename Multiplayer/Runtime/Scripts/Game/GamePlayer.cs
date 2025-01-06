using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GamePlayer : NetworkBehaviour
    {
        public ulong ConnectionId => OwnerClientId;
        public NetworkVariable<FixedString32Bytes> PlayerId;
        public NetworkVariable<FixedString32Bytes> PlayerName;

        GameManager _gameManager;

        GameManager GameManager
        {
            get
            {
                if (_gameManager == null)
                    _gameManager = FindFirstObjectByType<GameManager>();
                return _gameManager;
            }
        }

        void Awake()
        {
            PlayerId = new();
            PlayerName = new();
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (NetworkManager.Singleton.IsHost)
                InitializePlayer();
        }

        void InitializePlayer()
        {
            if (!IsServer) return;
            PlayerName.Value = PlayerConnectionsManager.Instance.GetName(OwnerClientId);
            ;
            PlayerId.Value = PlayerConnectionsManager.Instance.GetPlayerId(OwnerClientId);

            gameObject.name = "NetworkPlayer (" + PlayerName.Value + ")";
            PlayerConnectionsManager.Instance.RegisterPlayerAndRemoveDuplicates(this);

            Debug.LogWarning("Player Spawn Completed for " + OwnerClientId + ": " + PlayerName.Value + " on " +
                             (IsServer ? "Server" : "Client") + ". PlayerId=" + PlayerId.Value);
        }
    }
}