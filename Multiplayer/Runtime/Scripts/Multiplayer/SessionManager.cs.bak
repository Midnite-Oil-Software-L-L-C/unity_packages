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
                if (!_gameSessionManager && _enableDebugLog)
                {
                    Logwin.LogError("SessionManager", 
                        "No GameSessionManager found in scene, please add one or override InitializeSession in a derived class.", 
                        "Multiplayer");
                }
            }
            if (UnityServices.State != ServicesInitializationState.Uninitialized) return;
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
        {
            var properties = await Task.Run(() =>
            {
                var playerName = AuthenticationManager.Instance.PlayerName;
                var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
                return new Dictionary<string, PlayerProperty> { { PlayerNamePropertyKey, playerNameProperty } };
            });

            return properties;
        }

        public async void StartSessionAsHost(string sessionName)
        {
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

                _activeSession = await MultiplayerService.Instance.CreateSessionAsync(options.WithRelayNetwork());
                if (_enableDebugLog)
                {
                    Logwin.Log("SessionManager", $"Created session {_activeSession.Name} with ID: {_activeSession.Id}", "Multiplayer");
                }
                OnSessionJoined?.Invoke(_activeSession);
            }
            catch (RelayServiceException e)
            {
                if (_enableDebugLog) Logwin.LogError("SessionManager", $"Failed to get Relay join code: {e}", "Multiplayer");
            }
            catch (Exception e)
            {
                if (_enableDebugLog) Logwin.LogError("SessionManager", $"Failed to create session: {e}", "Multiplayer");
            }
        }

        public async Task JoinSessionById(string sessionId)
        {
            if (_enableDebugLog) Logwin.Log("SessionManager", $"Joining session with id: {sessionId}", "Multiplayer");
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
                    Logwin.Log("SessionManager", $"Leaving session {_activeSession.Name} with ID: {_activeSession.Id}",
                        "Multiplayer");
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