using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyEntry : MonoBehaviour
    {
        [SerializeField] TMP_Text _name;
        [SerializeField] TMP_Text _playerCount;
        [SerializeField] Button _joinButton;

        Unity.Services.Lobbies.Models.Lobby _lobby;

        void Start()
        {
            _joinButton.onClick.AddListener(TryJoin);
        }

        public void Bind(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            _lobby = lobby;
            _name.SetText(lobby.Name);
            _playerCount.SetText(lobby.Players.Count + " / " + lobby.MaxPlayers);

            if (_lobby.Id == LobbyManager.Instance.CurrentLobby?.Id ||
                _lobby.AvailableSlots < 1)
            {
                _joinButton.interactable = false;
            }
        }

        async void TryJoin()
        {
            try
            {
                await LobbyManager.Instance.JoinLobby(_lobby);
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception);
            }
        }
    }
}