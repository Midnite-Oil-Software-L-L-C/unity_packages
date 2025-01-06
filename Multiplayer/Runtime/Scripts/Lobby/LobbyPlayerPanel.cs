using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyPlayerPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text _playerNameText;
        [SerializeField] Image _playerReadyIndicator;
        [SerializeField] Button _kickPlayerButton, _playerReadyButton;
        [SerializeField] Material _playerReadyMaterial, _notReadyMaterial;
        [SerializeField] Color _readyColor, _notReadyColor;

        public Player Player { get; private set; }

        void Awake()
        {
            _notReadyMaterial.color = _notReadyColor;
            _playerReadyMaterial.color = _readyColor;
            SetPlayerReadyIndicatorColor(false);
        }

        void OnDisable()
        {
            _playerReadyButton.onClick.RemoveListener(OnPlayerReadyClicked);
            _kickPlayerButton.onClick.RemoveListener(OnKickPlayerClicked);
        }

        public void Bind(Player player)
        {
            Player = player;
            if (player.Data == null)
            {
                _playerNameText.text = "<no data>";
                SetPlayerReadyIndicatorColor(false);
                _kickPlayerButton.gameObject.SetActive(false);
                return;
            }

            _playerNameText.text = player.Data.TryGetValue("name", out var playerName)
                ? playerName.Value
                : "<no name>";
            if (player.Data.TryGetValue("isReady", out var playerReady))
            {
                Debug.Log($"binding player {playerName?.Value} ready state: {playerReady.Value}");
                SetPlayerReadyIndicatorColor(playerReady.Value == "true");
            }
            else
            {
                SetPlayerReadyIndicatorColor(false);
            }

            if (LobbyManager.Instance.IsLocalPlayerLobbyHost &&
                player.Data.TryGetValue("isHost", out var isHost) &&
                isHost.Value == "false")
            {
                _kickPlayerButton.gameObject.SetActive(true);
                _kickPlayerButton.onClick.AddListener(OnKickPlayerClicked);
            }
            else
            {
                _kickPlayerButton.gameObject.SetActive(false);
                if (player.Id == AuthenticationService.Instance.PlayerId &&
                    playerReady == null || playerReady?.Value == "false")
                {
                    _playerReadyButton.gameObject.SetActive(true);
                    _playerReadyButton.onClick.AddListener(OnPlayerReadyClicked);
                }
                else
                {
                    _playerReadyButton.gameObject.SetActive(false);
                }
            }
        }

        async void OnKickPlayerClicked()
        {
            var lobbyId = LobbyManager.Instance?.CurrentLobby?.Id;
            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogError("No lobby to kick player from");
                return;
            }

            _kickPlayerButton.gameObject.SetActive(false);
            await Lobbies.Instance.RemovePlayerAsync(lobbyId, Player.Id);
        }

        async void OnPlayerReadyClicked()
        {
            _playerReadyButton.gameObject.SetActive(false);
            await LobbyManager.Instance.SetClientReadyState(Player.Id);
        }

        void SetPlayerReadyIndicatorColor(bool isReady)
        {
            _playerReadyIndicator.material = isReady ? _playerReadyMaterial : _notReadyMaterial;
        }
    }
}
