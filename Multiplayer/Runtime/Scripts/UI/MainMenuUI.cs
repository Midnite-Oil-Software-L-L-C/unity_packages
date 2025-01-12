using MidniteOilSoftware.Multiplayer.Lobby;
using TMPro;
using UnityEngine;

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
        [SerializeField] TMP_Text _playerIdText ,_statusText, _gameNameText;

        void Start()
        {
            _mainMenuUI.SetActive(true);
            _lobbyPanel.Initialize();
            _currentLobbyPanel.gameObject.SetActive(false);
            LobbyManager.Instance.OnJoinedLobby += ShowCurrentLobby;
            LobbyManager.Instance.OnLeftLobby += ShowLobbyPanel;
            _authenticationPanel.PlayerQuit += ExitGame;
            _authenticationPanel.PlayerSignedIn += PlayerLoggedIn;
            _authenticationPanel.gameObject.SetActive(true);
        }

        void LateUpdate()
        {
            _gameNameText.SetText(LobbyManager.Instance.CurrentLobby?.Name ?? string.Empty);
        }

        void PlayerLoggedIn(string playerName)
        {
            _playerIdText.SetText(playerName);
            ShowLobbyPanel();
        }

        void ShowLobbyPanel()
        {
            _authenticationPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _currentLobbyPanel.gameObject.SetActive(false);
        }

        void ShowCurrentLobby(Unity.Services.Lobbies.Models.Lobby _)
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