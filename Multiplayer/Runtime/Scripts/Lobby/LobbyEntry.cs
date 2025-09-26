using MidniteOilSoftware.Core;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class LobbyEntry : MonoBehaviour
    {
        [SerializeField] TMP_Text _name;
        [SerializeField] TMP_Text _playerCount;
        [SerializeField] Button _joinButton;
        [SerializeField] bool _enableDebugLog = true;

        ISessionInfo _session;

        void Start()
        {
            _joinButton.onClick.AddListener(TryJoin);
        }

        public void Bind(ISessionInfo session)
        {
            _session = session;
            _name.SetText(session.Name);
            _playerCount.SetText((session.MaxPlayers - session.AvailableSlots) + " / " + session.MaxPlayers);

            if (_enableDebugLog)
            {
                Logwin.Log("LobbyEntry",
                    $"LobbyEntry.Bind(): Binding entry to session {session.Name}:{session.Id}, isLocked: {session.IsLocked}, available slots: {session.AvailableSlots}",
                    "Multiplayer");
            }
            if (session.AvailableSlots < 1 || session.IsLocked || string.IsNullOrEmpty(session.Id))
            {
                _joinButton.interactable = false;
            }
        }

        async void TryJoin()
        {
            if (string.IsNullOrEmpty(_session?.Id))
            {
                if (_enableDebugLog)
                {
                    Logwin.LogError("LobbyEntry", 
                        "LobbyEntry.TryJoin(): Session ID is null or empty, cannot join session.",
                        "Multiplayer");
                }
                return;
            }
            await SessionManager.Instance.JoinSessionById(_session.Id);
        }

    }
}