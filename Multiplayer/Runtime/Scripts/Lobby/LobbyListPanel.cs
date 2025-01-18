using System.Collections.Generic;
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
            _autoRefreshToggle.onValueChanged.RemoveAllListeners();
            _autoRefreshToggle.onValueChanged.AddListener(LobbyManager.Instance.ToggleAutoRefreshLobbies);
            _hostButton.onClick.RemoveAllListeners();
            _hostButton.onClick.AddListener(LobbyManager.Instance.HostLobbyAsync);
            _refreshButton.onClick.RemoveAllListeners();
            _refreshButton.onClick.AddListener(HandleRefreshLobbyClick);
            #if UNITY_WEBGL
            _backButton.gameObject.SetActive(false);
            #else
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(HandleBackButtonClick);
            _backButton.gameObject.SetActive(true);
            #endif
            LobbyManager.Instance.ToggleAutoRefreshLobbies(_autoRefreshToggle.isOn);
            LobbyManager.Instance.OnLobbiesUpdated += UpdateLobbiesUI;
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