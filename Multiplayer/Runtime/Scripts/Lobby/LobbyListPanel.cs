using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Pool;
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
        ObjectPool<LobbyEntry> _lobbyEntryPool;
        readonly List<LobbyEntry> _activeLobbyEntries = new();
        const float RefreshRate = 2.0f;

        public void Initialize()
        {
            InitializeObjectPool();
            
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
            ReleaseAllActiveLobbyEntries();
            _lobbyEntryPool?.Clear();
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
                if (_enableDebugLog) Debug.LogError($"LobbyListPanel:Multiplayer-Error querying sessions: {e}");
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

        void InitializeObjectPool()
        {
            _lobbyEntryPool = new ObjectPool<LobbyEntry>(
                createFunc: () =>
                {
                    var entry = Instantiate(_lobbyEntryPrefab, _lobbiesRoot);
                    entry.gameObject.SetActive(false);
                    return entry;
                },
                actionOnGet: entry =>
                {
                    entry.gameObject.SetActive(true);
                },
                actionOnRelease: entry =>
                {
                    entry.gameObject.SetActive(false);
                },
                actionOnDestroy: entry =>
                {
                    if (entry != null)
                    {
                        Destroy(entry.gameObject);
                    }
                },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 50
            );
        }

        void ReleaseAllActiveLobbyEntries()
        {
            foreach (var entry in _activeLobbyEntries)
            {
                if (entry != null)
                {
                    _lobbyEntryPool.Release(entry);
                }
            }
            _activeLobbyEntries.Clear();
        }

        void UpdateLobbiesUI(IList<ISessionInfo> sessions)
        {
            ReleaseAllActiveLobbyEntries();

            foreach (var session in sessions)
            {
                var lobbyEntry = _lobbyEntryPool.Get();
                lobbyEntry.Bind(session);
                _activeLobbyEntries.Add(lobbyEntry);
            }
        }
    }
}