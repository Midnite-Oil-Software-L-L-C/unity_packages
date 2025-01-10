using System;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Lobby;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")] [SerializeField] GameObject _mainMenuUI, _loginPanel;
        [SerializeField] LobbyListPanel _lobbyPanel;
        [SerializeField] CurrentLobbyPanel _currentLobbyPanel;

        [Header("Input Fields")] [SerializeField]
        TMP_InputField _usernameInput;

        [SerializeField] TMP_InputField _passwordInput;

        [Header("Text Fields")] 
        [SerializeField] TMP_Text _playerIdText ,_statusText, _gameNameText;

        [Header("Buttons")] [SerializeField] Button _loginButton;
        [SerializeField] Button _registerButton;
        [SerializeField] Button _cancelButton;

        void Start()
        {
            SetupAuthentication();
            _mainMenuUI.SetActive(true);
            _lobbyPanel.Initialize();
            _currentLobbyPanel.gameObject.SetActive(false);
            LobbyManager.Instance.OnJoinedLobby += ShowCurrentLobby;
            LobbyManager.Instance.OnLeftLobby += ShowLobbyPanel;
        }

        void LateUpdate()
        {
            _gameNameText.SetText(LobbyManager.Instance.CurrentLobby?.Name ?? string.Empty);
        }

        void SetupAuthentication()
        {
            AuthenticationManager.Instance.OnSignedIn += HandlePlayerLoggedIn;
            AuthenticationManager.Instance.OnSigninFailed += ShowLoginPanel;
            _loginButton.onClick.AddListener(Login);
            _registerButton.onClick.AddListener(Register);
            _cancelButton.onClick.AddListener(ExitGame);
            ShowLoginPanel(null);
        }

        void ShowLoginPanel(RequestFailedException _)
        {
            // @todo replace with settings manager
            var userName = PlayerPrefs.GetString("username");

#if UNITY_EDITOR
            userName = FindFirstObjectByType<MPPMManager>().ProfileName ?? userName;
            var password = PlayerPrefs.GetString("password");
            if (!string.IsNullOrWhiteSpace(password))
                _passwordInput.text = password;
            _passwordInput.text = password;
#endif

            if (!string.IsNullOrWhiteSpace(userName))
            {
                _usernameInput.text = userName;
            }

            _loginPanel.SetActive(true);
        }

        void HideLoginPanel()
        {
            _loginPanel.SetActive(false);
        }

        async void Login()
        {
            HideLoginPanel();
            var result =
                await AuthenticationManager.Instance.SignInWithUserNameAndPasswordAsync(
                    _usernameInput.text, _passwordInput.text);
            if (!result.Success)
            {
                ShowLoginPanel(null);
                _statusText.SetText(result.Message);
                return;
            }

            SaveUsernameAndPassword();
        }

        void HandlePlayerLoggedIn()
        {
            ShowLobbyPanel();
            _playerIdText.SetText(AuthenticationService.Instance.PlayerId);
        }

        async void Register()
        {
            _registerButton.gameObject.SetActive(false);

            var result = await AuthenticationManager.Instance.RegisterWithUserNameAndPasswordAsync(
                _usernameInput.text, _passwordInput.text);
            if (!result.Success)
            {
                _registerButton.gameObject.SetActive(true);
                _statusText.SetText(result.Message);
                return;
            }

            SaveUsernameAndPassword();
        }

        void ShowLobbyPanel()
        {
            _lobbyPanel.gameObject.SetActive(true);
            _currentLobbyPanel.gameObject.SetActive(false);
        }

        void ShowCurrentLobby(Unity.Services.Lobbies.Models.Lobby _)
        {
            _lobbyPanel.gameObject.SetActive(false);
            _currentLobbyPanel.gameObject.SetActive(true);
            _currentLobbyPanel.Initialize();
        }

        void SaveUsernameAndPassword()
        {
            // @todo replace with settings manager
            PlayerPrefs.SetString("username", _usernameInput.text);
#if UNITY_EDITOR
            PlayerPrefs.SetString("password", _passwordInput.text);
#endif
            PlayerPrefs.Save();
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