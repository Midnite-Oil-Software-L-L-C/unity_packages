using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using MidniteOilSoftware.Core;
using System.Threading.Tasks;
using MidniteOilSoftware.Multiplayer;
using MidniteOilSoftware.Multiplayer.Authentication;
using Unity.Services.Relay;
using UnityEngine;

namespace MidniteOilSoftware
{
    public class SessionManager : SingletonMonoBehaviour<SessionManager>
    {
        [SerializeField] GameSessionManager _gameSessionManager;
        public event System.Action<ISession> OnSessionJoined;
        public event System.Action OnSessionLeft;

        private ISession _activeSession;
        public ISession ActiveSession => _activeSession;

        const string PlayerNamePropertyKey = "playerName";
        const string SessionCodePropertyKey = "sessionCode";

        protected override async void Start()
        {
            base.Start();
            if (!_gameSessionManager)
            {
                _gameSessionManager = FindFirstObjectByType<GameSessionManager>();
                if (!_gameSessionManager)
                {
                    Debug.LogError("SessionManager:Multiplayer-No GameSessionManager found in scene, please add one or override InitializeSession in a derived class.");
                }
            }
            if (UnityServices.State != ServicesInitializationState.Uninitialized) return;
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
        {
            var playerName = AuthenticationManager.Instance.PlayerName;
            var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
            var properties = new Dictionary<string, PlayerProperty> { { PlayerNamePropertyKey, playerNameProperty } };
            return Task.FromResult(properties);
        }

        public async void StartSessionAsHost(string sessionName)
        {
            if (_enableDebugLog) Debug.Log($"SessionManager:Multiplayer-Creating session with name: {sessionName}. Calling GetPlayerProperties()");
            var playerProperties = await GetPlayerProperties();
            try
            {
                var options = new SessionOptions
                {
                    Name = sessionName,
                    MaxPlayers = _gameSessionManager ? _gameSessionManager.MaxPlayers : 4,
                    IsLocked = false,
                    IsPrivate = false,
                    PlayerProperties = playerProperties
                };

                if (_enableDebugLog)
                {
                    Debug.Log("SessionManager:Multiplayer-Calling CreateSessionAsync with Relay Network");
                }
                _activeSession = await MultiplayerService.Instance.CreateSessionAsync(options.WithRelayNetwork());
                if (_enableDebugLog)
                {
                    Debug.Log($"SessionManager:Multiplayer-Created session {_activeSession.Name} with ID: {_activeSession.Id}");
                }
                OnSessionJoined?.Invoke(_activeSession);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"SessionManager:Multiplayer-Failed to get Relay join code: {e}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SessionManager:Multiplayer-Failed to create session: {e}");
            }
        }

        public async Task JoinSessionById(string sessionId)
        {
            if (_enableDebugLog) Debug.Log($"SessionManager:Multiplayer-Joining session with id: {sessionId}");
            var playerProperties = await GetPlayerProperties();
            var options = new JoinSessionOptions
            {
                PlayerProperties = playerProperties
            };
            _activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, options);
            OnSessionJoined?.Invoke(_activeSession);
        }

        public async void LeaveSession()
        {
            if (_activeSession == null) return;
            try
            {
                if (_enableDebugLog)
                {
                    Debug.Log($"SessionManager:Multiplayer-Leaving session {_activeSession.Name} with ID: {_activeSession.Id}");
                }
                await _activeSession.LeaveAsync();
            }
            catch
            {
                // Ignored as we are exiting the game
            }
            finally
            {
                _activeSession = null;
                OnSessionLeft?.Invoke();
            }
        }
    }
}