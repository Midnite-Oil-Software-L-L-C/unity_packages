using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyListPanel : MonoBehaviour
    {
        [SerializeField] Transform _lobbiesRoot;
        [SerializeField] LobbyEntry _lobbyEntryPrefab;
        [SerializeField] Toggle _autoRefreshToggle;
        [SerializeField] Button _hostButton, _refreshButton, _backButton;

        public void Initialize()
        {
            Debug.Assert(_autoRefreshToggle != null, "_autoRefreshToggle is null");
            Debug.Assert(LobbyManager.Instance != null, "LobbyManager.Instance is null");

            // Remove existing listeners to prevent duplicates
            _autoRefreshToggle.onValueChanged.RemoveAllListeners();
            _autoRefreshToggle.onValueChanged.AddListener(LobbyManager.Instance.ToggleAutoRefreshLobbies);

            LobbyManager.Instance.ToggleAutoRefreshLobbies(_autoRefreshToggle.isOn);
            LobbyManager.Instance.OnLobbiesUpdated += UpdateLobbiesUI;
            _hostButton.onClick.AddListener(LobbyManager.Instance.HostLobbyAsync);
            _refreshButton.onClick.AddListener(HandleRefreshLobbyClick);
            _backButton.onClick.AddListener(HandleBackButtonClick);
        }

        void OnDestroy()
        {
            if (LobbyManager.Instance)
                LobbyManager.Instance.OnLobbiesUpdated -= UpdateLobbiesUI;
            _hostButton.onClick.RemoveAllListeners();
            _refreshButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
            _autoRefreshToggle.onValueChanged.RemoveAllListeners();
        }

        async void HandleRefreshLobbyClick()
        {
            await LobbyManager.Instance.RefreshLobbiesAsync();
        }

        void HandleBackButtonClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        void UpdateLobbiesUI(List<Unity.Services.Lobbies.Models.Lobby> lobbies)
        {
            for (var i = _lobbiesRoot.childCount - 1; i >= 0; i--)
                Destroy(_lobbiesRoot.GetChild(i).gameObject);

            foreach (var lobby in lobbies)
            {
                var lobbyPanel = Instantiate(_lobbyEntryPrefab, _lobbiesRoot);
                lobbyPanel.Bind(lobby);
            }
        }
    }
}