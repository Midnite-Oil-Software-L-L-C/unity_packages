using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Authentication;
using MidniteOilSoftware.Multiplayer.Events;
using MidniteOilSoftware.Multiplayer.Lobby;
using TMPro;
using UnityEngine;
using Unity.Services.Multiplayer;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] GameObject _mainMenuUI;
        [SerializeField] AuthenticationPanelUI _authenticationPanel;
        [SerializeField] LobbyListPanel _lobbyPanel;
        [SerializeField] CurrentLobbyPanel _currentLobbyPanel;

        [Header("Text Fields")] 
        [SerializeField] TMP_Text _playerIdText, _playerNameText, _statusText, _gameNameText;

        void Start()
        {
            _mainMenuUI.SetActive(true);

            _currentLobbyPanel.gameObject.SetActive(false);

            // New event subscriptions for the SessionManager
            SessionManager.Instance.OnSessionJoined += ShowCurrentLobby;
            SessionManager.Instance.OnSessionLeft += ShowLobbyPanel;

            _authenticationPanel.PlayerQuit += ExitGame;
            _authenticationPanel.PlayerSignedIn += PlayerLoggedIn;
            _authenticationPanel.gameObject.SetActive(true);
            EventBus.Instance.Subscribe<LeftGameEvent>(PlayerLeftGame);
        }

        void PlayerLeftGame(LeftGameEvent e)
        {
            Debug.Log($"Player left the game");
            if (SessionManager.Instance.ActiveSession != null)
            {
                ShowCurrentLobby(SessionManager.Instance.ActiveSession);
            }
            else
            {
                ShowLobbyPanel();
            }
        }

        void LateUpdate()
        {
            _gameNameText.SetText(SessionManager.Instance.ActiveSession?.Name ?? string.Empty);
        }

        void PlayerLoggedIn(string playerId, string playerName)
        {
            Debug.Log($"{playerName} logged in with Player ID: {playerId}");
            _playerIdText.SetText(playerId);
            _playerNameText.SetText(playerName);
            try
            {
                _lobbyPanel.Initialize();
            }
            catch (System.Exception ex)
            {
                // I want this shown in the browser developer tools console for WebGL builds
                Debug.LogError($"Error initializing LobbyListPanel: {ex.Message}", this);
            }
            ShowLobbyPanel();
        }

        void ShowLobbyPanel()
        {
            _authenticationPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _currentLobbyPanel.gameObject.SetActive(false);
        }

        void ShowCurrentLobby(ISession _)
        {
            _lobbyPanel.gameObject.SetActive(false);
            _currentLobbyPanel.gameObject.SetActive(true);
            _currentLobbyPanel.Initialize();
        }

        void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}