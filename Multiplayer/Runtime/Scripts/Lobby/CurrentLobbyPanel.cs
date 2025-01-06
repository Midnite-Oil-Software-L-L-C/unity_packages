using System;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class CurrentLobbyPanel : MonoBehaviour
    {
        [SerializeField] GameObject _gameNameComponent, _startingGamePanel;
        [SerializeField] DynamicInputFieldWithConfirmButton _editGameName;
        [SerializeField] TMP_Text _gameNameText, _playersCountText;
        [SerializeField] Transform _playersRoot;
        [SerializeField] Button _leaveGameButton;
        [SerializeField] Button _renameGameButton, _startGameButton;
        [SerializeField] LobbyPlayerPanel _playerPanelPrefab;
        [SerializeField] Color _readyColor, _notReadyColor;

        Timer _refreshTimer;

        public void Initialize()
        {
            Debug.Log($"Initialing CurrentLobbyPanel", this);
            _gameNameText.text = LobbyManager.Instance.CurrentLobby.Name;
            _gameNameComponent.SetActive(true);
            _editGameName.gameObject.SetActive(false);
            _startingGamePanel.SetActive(false);
            SubscribeToButtonHandlers();
            SubscribeToLobbyEvents();
            InitializeRefreshTimer();
        }

        void OnEnable()
        {
            UpdateButtons();
            UpdatePlayers();
        }

        void OnDisable()
        {
            UnsubscribeFromButtonHandlers();
            UnsubscribeFromLobbyEvents();
            ReleaseRefreshTimer();
        }

        void SubscribeToButtonHandlers()
        {
            _leaveGameButton.onClick.AddListener(() =>
            {
                Debug.Log($"Leaving lobby {LobbyManager.Instance.CurrentLobby.Name}");
                LobbyManager.Instance.LeaveCurrentLobby();
            });
            _renameGameButton.onClick.AddListener(EditGameName);
            _startGameButton.onClick.AddListener(TryStartGame);
        }

        void UnsubscribeFromButtonHandlers()
        {
            _leaveGameButton.onClick.RemoveAllListeners();
            _renameGameButton.onClick.RemoveAllListeners();
            _startGameButton.onClick.RemoveAllListeners();
        }

        void SubscribeToLobbyEvents()
        {
            LobbyManager.Instance.OnJoinedLobby += OnJoinedLobby;
            LobbyManager.Instance.OnLeftLobby += OnLeftLobby;
            LobbyManager.Instance.OnCurrentLobbyUpdated += CurrentLobbyUpdated;
        }

        void UnsubscribeFromLobbyEvents()
        {
            Debug.Log("Unsubscribing from lobby events");
            if (!LobbyManager.Instance) return;
            LobbyManager.Instance.OnJoinedLobby -= OnJoinedLobby;
            LobbyManager.Instance.OnLeftLobby -= OnLeftLobby;
            LobbyManager.Instance.OnCurrentLobbyUpdated -= CurrentLobbyUpdated;
        }

        void InitializeRefreshTimer()
        {
            _refreshTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
            _refreshTimer.OnTimerStop += CurrentLobbyUpdated;
            _refreshTimer.Start(0.5f);
        }

        void ReleaseRefreshTimer()
        {
            _refreshTimer.OnTimerStop -= CurrentLobbyUpdated;
            TimerManager.Instance.ReleaseTimer<CountdownTimer>(_refreshTimer);
        }

        void UpdateButtons()
        {
            _startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLocalPlayerLobbyHost);
            _startGameButton.interactable = LobbyManager.Instance.IsLocalPlayerLobbyHost &&
                                            LobbyManager.Instance.CurrentLobby.AllPlayersReady();
            _renameGameButton.interactable = LobbyManager.Instance.IsLocalPlayerLobbyHost;
        }

        #region Lobby events

        void OnJoinedLobby(Unity.Services.Lobbies.Models.Lobby game)
        {
            gameObject.SetActive(true);
            _gameNameText.text = game.Name;
            HandleLobbyStateChanged();
        }

        void OnLeftLobby()
        {
            Debug.Log("Left lobby");
            gameObject.SetActive(false);
        }

        void CurrentLobbyUpdated()
        {
            _gameNameText.text = LobbyManager.Instance.CurrentLobby.Name;
            HandleLobbyStateChanged();
        }

        void HandleLobbyStateChanged()
        {
            UpdatePlayers();
            UpdateButtons();
        }

        #endregion

        #region Button events

        void EditGameName()
        {
            _gameNameComponent.SetActive(false);
            _editGameName.EditConfirmed += OnGameNameChanged;
            _editGameName.EditCancelled += OnEditGameNameCancelled;
            _editGameName.gameObject.SetActive(true);
            _editGameName.Initialize(_gameNameText.text);
        }

        void OnEditGameNameCancelled()
        {
            DisableEditGameComponent();
        }

        void OnGameNameChanged(string newGameName)
        {
            if (string.Compare(newGameName, _gameNameText.text, StringComparison.InvariantCulture) != 0)
            {
                LobbyManager.Instance.RenameLobby(newGameName);
            }

            DisableEditGameComponent();
        }

        void DisableEditGameComponent()
        {
            _editGameName.gameObject.SetActive(false);
            _editGameName.EditConfirmed -= OnGameNameChanged;
            _editGameName.EditCancelled -= OnEditGameNameCancelled;
            _gameNameComponent.SetActive(true);
        }

        async void TryStartGame()
        {
            try
            {
                _startGameButton.interactable = false;
                _startingGamePanel.SetActive(true);
                await LobbyManager.Instance.RequestStartGame();
                _startingGamePanel.SetActive(false);
                gameObject.SetActive(false);
            }
            catch (Exception e)
            {
                _startingGamePanel.SetActive(false);
                _startGameButton.interactable = true;
                Debug.LogError(e);
            }
        }

        #endregion Button events

        void UpdatePlayers()
        {
            Debug.Log($"Lobby {LobbyManager.Instance.CurrentLobby.Name} Updated", this);
            for (var i = _playersRoot.childCount - 1; i >= 0; i--)
                Destroy(_playersRoot.GetChild(i).gameObject);

            _playersCountText.text = LobbyManager.Instance.CurrentLobby.Players.Count +
                                     " / " + LobbyManager.Instance.CurrentLobby.MaxPlayers;
            foreach (var player in LobbyManager.Instance.CurrentLobby.Players)
            {
                var playerPanel = Instantiate(_playerPanelPrefab, _playersRoot);
                playerPanel.Bind(player);
            }
        }
    }
}
