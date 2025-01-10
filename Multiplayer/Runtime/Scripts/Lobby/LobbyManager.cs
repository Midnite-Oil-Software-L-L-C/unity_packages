using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MidniteOilSoftware.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
    {
        [SerializeField] int _maxPlayers = 4;
        public Unity.Services.Lobbies.Models.Lobby CurrentLobby { get; private set; }

        float _lastLobbyRefreshTime;
        List<Unity.Services.Lobbies.Models.Lobby> _lobbies;
        Coroutine _refreshLobbiesRoutine;
        Timer _heartBeatTimer;
        Timer _refreshTimer;
        bool _lobbiesModified;
        ILobbyEvents _lobbyEvents;

        const string RelayCodeKey = "relayCode";
        const string IsReadyKey = "isReady";
        const string IsHostKey = "isHost";
        const string LogWinCategory = "LobbyManger";

        public bool IsLocalPlayerLobbyHost =>
            CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance?.PlayerId;

        public event Action<List<Unity.Services.Lobbies.Models.Lobby>> OnLobbiesUpdated;
        public event Action<Unity.Services.Lobbies.Models.Lobby> OnJoinedLobby;
        public event Action OnCurrentLobbyUpdated;
        public event Action OnLeftLobby;

        public async Task<Unity.Services.Lobbies.Models.Lobby> HostLobby()
        {
            var options = new CreateLobbyOptions()
            {
                Data = new()
            };
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(
                AuthenticationService.Instance.Profile + "'s Game",
                _maxPlayers,
                options);

            await UpdatePlayerLobbyData(true);
            _lobbyEvents = await SubscribeToLobbyEventsAsync();

            OnJoinedLobby?.Invoke(CurrentLobby);

            StartHeartbeatTimer();
            return CurrentLobby;
        }

        public async Task JoinLobby(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            await UpdatePlayerLobbyData();
            _lobbyEvents = await SubscribeToLobbyEventsAsync();
            if (CurrentLobby.Data != null && CurrentLobby.Data.TryGetValue(RelayCodeKey, out var relayCode))
                await SetRelayClientData(relayCode.Value);

            OnJoinedLobby?.Invoke(CurrentLobby);
        }

        public async void LeaveCurrentLobby()
        {
            if (CurrentLobby == null)
                return;

            StopHeartbeatTimer();
            await UnsubscribeFromLobbyEvents();
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
            CurrentLobby = null;
            OnLeftLobby?.Invoke();
        }

        public async void RenameLobby(string lobbyName)
        {
            if (!IsLocalPlayerLobbyHost)
                return;

            var options = new UpdateLobbyOptions
            {
                Name = lobbyName
            };
            CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);
            OnCurrentLobbyUpdated?.Invoke();
        }

        public async void JoinLobbyById(string lobbyId)
        {
            await RefreshLobbiesAsync();
            var lobby = _lobbies.FirstOrDefault(t => t.Id == lobbyId);
            if (lobby == null)
            {
                Debug.LogError($"Unable to find lobby {lobbyId}");
                return;
            }

            await JoinLobby(lobby);
        }

        public async void HostLobbyAsync()
        {
            await HostLobby();
        }

        public async Task SetClientReadyState(string playerId)
        {
            var options = new UpdatePlayerOptions()
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    [IsReadyKey] = new(PlayerDataObject.VisibilityOptions.Member, "true")
                }
            };
            if (AuthenticationService.Instance == null)
            {
                Debug.LogError("AuthenticationService is null");
                return;
            }

            if (CurrentLobby == null)
            {
                Debug.LogError("CurrentLobby is null");
                return;
            }

            CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(
                CurrentLobby.Id,
                playerId,
                options);
            OnCurrentLobbyUpdated?.Invoke();
        }

        public async Task RequestStartGame()
        {
            string relayCode = null;
            try
            {
                relayCode = await GetRelayAllocation();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting relay allocation. {e}");
                return;
            }

            try
            {
                await PlayerConnectionsManager.Instance.StartHostOnServer();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error starting host. {e}");
                return;
            }

            try
            {
                await SetLobbyRelayCode(relayCode);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting relay code. {e}");
            }
        }

        void Update()
        {
            if (!_lobbiesModified) return;
            _lobbiesModified = false;
            OnLobbiesUpdated?.Invoke(_lobbies);
        }

        #region Lobbies callbacks

        void HandlePlayerJoined(List<LobbyPlayerJoined> playerJoins)
        {
            Debug.Log($"LobbyPlayerJoined: {playerJoins.Count}");
            foreach (var join in playerJoins)
            {
                Debug.Log($"Player {join.PlayerIndex}-{join.Player.Id} joined lobby.");
            }

            OnCurrentLobbyUpdated?.Invoke();
        }

        void HandlePlayerLeft(List<int> playerLeft)
        {
            Debug.Log($"LobbyPlayerLeft: {playerLeft.Count}");
            foreach (var player in playerLeft)
            {
                Debug.Log($"Player {player} left lobby.");
            }

            OnCurrentLobbyUpdated?.Invoke();
        }

        async void HandleKickedFromLobby()
        {
            Debug.Log("Kicked from lobby");
            await UnsubscribeFromLobbyEvents();
            CurrentLobby = null;
            OnLeftLobby?.Invoke();
        }

        void HandleLobbyDeleted()
        {
            Debug.Log("Lobby deleted");
            OnCurrentLobbyUpdated?.Invoke();
        }

        #endregion Lobbies callbacks

        void StartHeartbeatTimer()
        {
            _heartBeatTimer = TimerManager.Instance.CreateTimer<CountdownTimer>(15f);
            _heartBeatTimer.OnTimerStop += SendHeartbeatPing;
            _heartBeatTimer.Start();
        }

        void SendHeartbeatPing()
        {
            LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            _heartBeatTimer.Start(15);
        }

        public void ToggleAutoRefreshLobbies(bool autoRefreshEnabled)
        {
            if (autoRefreshEnabled)
                StartRefreshTimer();
            else if (_refreshLobbiesRoutine != null)
                StopRefreshTimer();
        }

        void StartRefreshTimer()
        {
            _refreshTimer = TimerManager.Instance.CreateTimer<CountdownTimer>(1.5f);
            _refreshTimer.OnTimerStop += RefreshLobbies;
            _refreshTimer.Start();
        }

        void RestartRefreshTimer()
        {
            if (_refreshTimer == null) return;
            _refreshTimer.OnTimerStop -= RefreshLobbies;
            _refreshTimer.OnTimerStop += RefreshLobbies;
            _refreshTimer.Start();
        }

        void StopRefreshTimer()
        {
            if (_refreshTimer == null) return;
            _refreshTimer.OnTimerStop -= RefreshLobbies;
            _refreshTimer.Stop();
            _refreshTimer = null;
        }

        void StopHeartbeatTimer()
        {
            if (_heartBeatTimer == null) return;
            _heartBeatTimer.OnTimerStop -= SendHeartbeatPing;
            _heartBeatTimer.Stop();
            _heartBeatTimer = null;
        }

        void RefreshLobbies()
        {
            Task.Run(async () =>
            {
                try
                {
                    await RefreshLobbiesAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    RestartRefreshTimer();
                }
            });
        }

        async Task UpdatePlayerLobbyData(bool isHost = false)
        {
            try
            {
                var options = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            "name", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: AuthenticationManager.Instance.PlayerName)
                        },
                        {
                            "id", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: AuthenticationManager.Instance.PlayerId)
                        },
                        {
                            IsReadyKey, new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Member,
                                value: isHost ? "true" : "false")
                        },
                        {
                            IsHostKey, new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Member,
                                value: isHost ? "true" : "false")
                        }
                    }
                };

                var playerId = AuthenticationService.Instance.PlayerId;
                CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, playerId, options);
                OnCurrentLobbyUpdated?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        public async Awaitable RefreshLobbiesAsync()
        {
            try
            {
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
                _lobbies = queryResponse.Results;
                _lobbiesModified = true;
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("401")) Debug.LogError(e);
            }
        }

        async void HandleCurrentLobbyChanged(ILobbyChanges changes)
        {
            if (CurrentLobby == null) return;
            Debug.Log($"Changes to lobby {CurrentLobby.Name}");
            changes.ApplyToLobby(CurrentLobby);

            if (changes.Data.Changed &&
                changes.Data.Value.TryGetValue(RelayCodeKey, out var relayCode))
            {
                Debug.Log($"'relayCode' set to '{relayCode.Value.Value}'");
                await SetRelayClientData(relayCode.Value.Value);
            }

            OnCurrentLobbyUpdated?.Invoke();
        }

        async Task<ILobbyEvents> SubscribeToLobbyEventsAsync()
        {
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += HandleCurrentLobbyChanged;
            callbacks.PlayerJoined += HandlePlayerJoined;
            callbacks.PlayerLeft += HandlePlayerLeft;
            callbacks.KickedFromLobby += HandleKickedFromLobby;
            callbacks.LobbyDeleted += HandleLobbyDeleted;
            return await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
        }

        async Task UnsubscribeFromLobbyEvents()
        {
            await _lobbyEvents.UnsubscribeAsync();
        }

        async Task SetRelayClientData(string relayCode)
        {
            var transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();
            var joinAllocation = await Relay.Instance.JoinAllocationAsync(relayCode);

            var endpoint = GetEndpointForAllocation(
                joinAllocation.ServerEndpoints,
                joinAllocation.RelayServer.IpV4,
                joinAllocation.RelayServer.Port,
                out var isSecure);

            transport.SetClientRelayData(
                AddressFromEndpoint(endpoint),
                endpoint.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                isSecure);

            await PlayerConnectionsManager.Instance.StartClient();
        }

        async Task<string> GetRelayAllocation()
        {
            var allocation = await Relay.Instance.CreateAllocationAsync(CurrentLobby.MaxPlayers);
            var relayCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var endpoint = GetEndpointForAllocation(
                allocation.ServerEndpoints,
                allocation.RelayServer.IpV4,
                allocation.RelayServer.Port,
                out bool isSecure);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, isSecure);

            return relayCode;
        }

        async Task SetLobbyRelayCode(string relayCode)
        {
            var options = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    [RelayCodeKey] = new(DataObject.VisibilityOptions.Public, relayCode)
                }
            };
            CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);
        }

        string AddressFromEndpoint(NetworkEndpoint endpoint)
        {
            return endpoint.Address.Split(':')[0];
        }

        /// <summary>
        /// Determine the server endpoint for connecting to the Relay server, for either an Allocation or a JoinAllocation.
        /// If DTLS encryption is available, and there's a secure server endpoint available, use that as a secure connection. Otherwise, just connect to the Relay IP unsecured.
        /// </summary>
        NetworkEndpoint GetEndpointForAllocation(
            List<RelayServerEndpoint> endpoints,
            string ip,
            int port,
            out bool isSecure)
        {
#if ENABLE_MANAGED_UNITYTLS
            foreach (RelayServerEndpoint endpoint in endpoints)
            {
                if (endpoint.Secure && endpoint.Network == RelayServerEndpoint.NetworkOptions.Udp)
                {
                    isSecure = true;
                    return NetworkEndpoint.Parse(endpoint.Host, (ushort)endpoint.Port);
                }
            }
#endif
            isSecure = false;
            return NetworkEndpoint.Parse(ip, (ushort)port);
        }
    }
}