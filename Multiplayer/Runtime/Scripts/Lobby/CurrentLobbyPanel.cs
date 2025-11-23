using System.Collections.Generic;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using MidniteOilSoftware.Multiplayer.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class CurrentLobbyPanel : MonoBehaviour
    {
        [SerializeField] GameObject _gameNameComponent, _startingGamePanel, _mainMenuBackground;
        [SerializeField] DynamicInputFieldWithConfirmButton _editGameName;
        [SerializeField] TMP_Text _gameNameText, _playersCountText;
        [SerializeField] Transform _playersRoot;
        [SerializeField] Button _leaveGameButton;
        [SerializeField] Button _renameGameButton, _startGameButton;
        [SerializeField] LobbyPlayerPanel _playerPanelPrefab;
        [SerializeField] Color _readyColor, _notReadyColor;
        [SerializeField] bool _enableDebugLog = true;

        Timer _refreshTimer;
        ObjectPool<LobbyPlayerPanel> _playerPanelPool;
        readonly List<LobbyPlayerPanel> _activePlayerPanels = new();

        public void Initialize()
        {
            Debug.Log($"Initialing CurrentLobbyPanel", this);
            InitializeObjectPool();
            _gameNameComponent.SetActive(true);
            _editGameName.gameObject.SetActive(false);
            _startingGamePanel.SetActive(false);
            SubscribeToButtonHandlers();
            SubscribeToGameEvents();
            InitializeRefreshTimer();
        }

        void OnEnable()
        {
            // Note: UpdateButtons() and UpdatePlayers() now need to use the SessionManager.ActiveSession
            // as the source of truth for player data and host status.
            UpdateButtons();
            UpdatePlayers();
        }

        void OnDisable()
        {
            UnsubscribeFromButtonHandlers();
            UnsuscribeFromGameEvents();
            ReleaseRefreshTimer();
            ReleaseAllActivePlayerPanels();
            _playerPanelPool?.Clear();
        }

        void SubscribeToButtonHandlers()
        {
            _leaveGameButton.onClick.AddListener(() =>
            {
                SessionManager.Instance.LeaveSession();
            });
            _startGameButton.onClick.AddListener(TryStartGame);
        }

        void SubscribeToGameEvents()
        {
            EventBus.Instance.Subscribe<GameStateChangedEvent>(HandleGameStateChanged);
        }

        void UnsuscribeFromGameEvents()
        {
            EventBus.Instance.Unsubscribe<GameStateChangedEvent>(HandleGameStateChanged);
        }

        void UnsubscribeFromButtonHandlers()
        {
            _leaveGameButton.onClick.RemoveAllListeners();
            _startGameButton.onClick.RemoveAllListeners();
        }

        void InitializeRefreshTimer()
        {
            _refreshTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
            _refreshTimer.OnTimerStop += UpdateLobbyData;
            _refreshTimer.Start(0.5f);
        }

        void ReleaseRefreshTimer()
        {
            _refreshTimer.OnTimerStop -= UpdateLobbyData;
            TimerManager.Instance.ReleaseTimer<CountdownTimer>(_refreshTimer);
        }

        void InitializeObjectPool()
        {
            _playerPanelPool = new ObjectPool<LobbyPlayerPanel>(
                createFunc: () =>
                {
                    var panel = Instantiate(_playerPanelPrefab, _playersRoot);
                    panel.gameObject.SetActive(false);
                    return panel;
                },
                actionOnGet: panel =>
                {
                    panel.gameObject.SetActive(true);
                },
                actionOnRelease: panel =>
                {
                    panel.gameObject.SetActive(false);
                },
                actionOnDestroy: panel =>
                {
                    if (panel != null)
                    {
                        Destroy(panel.gameObject);
                    }
                },
                collectionCheck: true,
                defaultCapacity: 4,
                maxSize: 10
            );
        }

        void ReleaseAllActivePlayerPanels()
        {
            foreach (var panel in _activePlayerPanels)
            {
                if (panel != null)
                {
                    _playerPanelPool.Release(panel);
                }
            }
            _activePlayerPanels.Clear();
        }
        
        void UpdateLobbyData()
        {
            if (SessionManager.Instance.ActiveSession == null) return;
            UpdatePlayers();
            UpdateButtons();
            // Restart timer
            _refreshTimer.Start(0.5f);
        }

        void UpdateButtons()
        {
            var activeSession = SessionManager.Instance.ActiveSession;
            var isHost = activeSession?.IsHost ?? false;
            var enoughPlayers = activeSession != null && activeSession.Players.Count >= activeSession.MaxPlayers;
            _startGameButton.gameObject.SetActive(isHost);
            _startGameButton.interactable = isHost && enoughPlayers;
        }

        #region Game events
        void HandleGameStateChanged(GameStateChangedEvent e)
        {
            switch (e.NewState)
            {
                case GameState.GameStarted:
                case GameState.WaitingForPlayers:
                case GameState.PlayerTurnStart:
                    gameObject.SetActive(false);
                    break;
            }
        }
        #endregion Game events

        #region Button events
        async void TryStartGame()
        {
            if (_enableDebugLog) Debug.Log("CurrentLobbyPanel:Multiplayer-TryStartGame called");
            _startGameButton.interactable = false;
            _startingGamePanel.SetActive(true);
           
            GameSessionManager.Instance.InitializeSession();
            _startingGamePanel.SetActive(false);
            _mainMenuBackground?.SetActive(false);
            gameObject.SetActive(false);
        }

        #endregion Button events

        void UpdatePlayers()
        {
            var activeSession = SessionManager.Instance.ActiveSession;
            if (activeSession == null) return;
            
            _gameNameText.text = activeSession.Name;
            
            ReleaseAllActivePlayerPanels();

            _playersCountText.text = activeSession.Players.Count + " / " + activeSession.MaxPlayers;
            
            foreach (var player in activeSession.Players)
            {
                var playerPanel = _playerPanelPool.Get();
                playerPanel.Bind(player);
                _activePlayerPanels.Add(playerPanel);
            }
        }
    }
}