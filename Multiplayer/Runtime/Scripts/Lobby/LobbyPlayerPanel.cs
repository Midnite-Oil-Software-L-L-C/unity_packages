using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyPlayerPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text _playerNameText;
        [SerializeField] Button _kickPlayerButton;

        IReadOnlyPlayer _player;

        void OnDisable()
        {
            _kickPlayerButton.onClick.RemoveListener(OnKickPlayerClicked);
        }

        // Refactored to accept IReadOnlyPlayer
        public void Bind(IReadOnlyPlayer player)
        {
            _player = player;

            if (player.Properties.TryGetValue("playerName", out var playerName))
            {
                _playerNameText.text = playerName.Value;
            }

            if (SessionManager.Instance.ActiveSession?.IsHost ?? false)
            {
                if (player.Id != AuthenticationService.Instance.PlayerId)
                {
                    _kickPlayerButton.gameObject.SetActive(true);
                    _kickPlayerButton.onClick.AddListener(OnKickPlayerClicked);
                }
                else
                {
                    _kickPlayerButton.gameObject.SetActive(false);
                }
            }
            else
            {
                _kickPlayerButton.gameObject.SetActive(false);
            }
        }

        async void OnKickPlayerClicked()
        {
            if (SessionManager.Instance.ActiveSession?.IsHost != true) return;

            _kickPlayerButton.gameObject.SetActive(false);
            try
            {
                await SessionManager.Instance.ActiveSession.AsHost().RemovePlayerAsync(_player.Id);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to kick player: {e}");
            }
        }
    }
}