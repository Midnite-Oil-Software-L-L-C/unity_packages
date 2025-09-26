using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Authentication;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyListPanel : MonoBehaviour
    {
        [SerializeField] Transform _lobbiesRoot;
        [SerializeField] LobbyEntry _lobbyEntryPrefab;
        [SerializeField] Toggle _autoRefreshToggle;
        [SerializeField] Button _hostButton, _refreshButton, _backButton;
        [Header("Debug settings")]
        [SerializeField] bool _enableDebugLog = false;
        
        Timer _refreshTimer;
        const float RefreshRate = 2.0f;

        public void Initialize()
        {
            _autoRefreshToggle.onValueChanged.RemoveAllListeners();
            _autoRefreshToggle.onValueChanged.AddListener(ToggleAutoRefresh);
            
            _hostButton.onClick.RemoveAllListeners();
            _hostButton.onClick.AddListener(() =>
            {
                SessionManager.Instance.StartSessionAsHost(AuthenticationManager.Instance.PlayerName + "'s Game");
            });

            _refreshButton.onClick.RemoveAllListeners();
            _refreshButton.onClick.AddListener(HandleRefreshLobbyClick);
            
            #if UNITY_WEBGL
            _backButton.gameObject.SetActive(false);
            #else
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(HandleBackButtonClick);
            _backButton.gameObject.SetActive(true);
            #endif

            if (_autoRefreshToggle.isOn)
            {
                StartAutoRefresh();
            }
        }
        
        void OnDestroy()
        {
            StopAutoRefresh();
            _hostButton.onClick.RemoveAllListeners();
            _refreshButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
            _autoRefreshToggle.onValueChanged.RemoveAllListeners();
        }

        void ToggleAutoRefresh(bool autoRefreshEnabled)
        {
            if (autoRefreshEnabled)
            {
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        }

        void StartAutoRefresh()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = TimerManager.Instance.CreateTimer<CountdownTimer>(RefreshRate);
                _refreshTimer.OnTimerStop += RefreshSessions;
            }
            _refreshTimer.Start();
        }

        void StopAutoRefresh()
        {
            _refreshTimer?.Stop(false);
        }

        void HandleRefreshLobbyClick()
        {
            RefreshSessions();
        }

        async void RefreshSessions()
        {
            try
            {
                var querySessionsOptions = new QuerySessionsOptions();
                QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(querySessionsOptions);
                UpdateLobbiesUI(results.Sessions);
            }
            catch (System.Exception e)
            {
                if (_enableDebugLog) Logwin.LogError("LobbyListPanel", $"Error querying sessions: {e}", "Multiplayer");
            }
            finally
            {
                // Restart the timer only if auto-refresh is enabled
                if (_autoRefreshToggle.isOn)
                {
                    _refreshTimer.Start(RefreshRate);
                }
            }
        }

        void HandleBackButtonClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // todo - refactor to use object pooling instead of destroying/instantiating
        void UpdateLobbiesUI(IList<ISessionInfo> sessions)
        {
            for (var i = _lobbiesRoot.childCount - 1; i >= 0; i--)
                Destroy(_lobbiesRoot.GetChild(i).gameObject);

            foreach (var session in sessions)
            {
                var lobbyPanel = Instantiate(_lobbyEntryPrefab, _lobbiesRoot);
                lobbyPanel.Bind(session);
            }
        }
    }
}