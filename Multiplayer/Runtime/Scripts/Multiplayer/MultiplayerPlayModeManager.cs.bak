using System;
using System.Collections;
using System.Linq;
using MidniteOilSoftware.Core;
using Unity.Multiplayer.Playmode;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using MidniteOilSoftware.Multiplayer.Authentication;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class MultiplayerPlayModeManager : MonoBehaviour
    {
        [SerializeField] bool _debugLog;
#if UNITY_EDITOR
        public string ProfileName { get; private set; }

        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            yield return StartCoroutine(InitializeMppm());
        }

        const string HostKey = "Host";
        const string ClientKey = "Client";
        const string ServerKey = "Server";
        const string LobbyHostKey = "Lobby Host";
        const string LobbyClientKey = "Lobby Client";

        IEnumerator InitializeMppm()
        {
            if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", "InitializeMPPM", "Multiplayer");
    
            var allTags = CurrentPlayer.ReadOnlyTags().ToArray();
            if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", $"All tags: [{string.Join(", ", allTags)}]", "Multiplayer");
    
            ProfileName = CurrentPlayer.ReadOnlyTags().Except(new[]
                {
                    LobbyHostKey, LobbyClientKey, HostKey, ClientKey, ServerKey
                })
                .FirstOrDefault();

            if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", $"ProfileName extracted: '{ProfileName}'", "Multiplayer");
    
            if (ProfileName != default)
            {
                AuthenticationService.Instance.SwitchProfile(ProfileName);
            }

            if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", $"Starting as {ProfileName}", "Multiplayer");

            if (CurrentPlayer.ReadOnlyTags().Contains(LobbyHostKey))
            {
                if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", "Starting Lobby Host Coroutine", "Multiplayer");
                StartCoroutine(LobbyHost());
            }
            if (CurrentPlayer.ReadOnlyTags().Contains(LobbyClientKey))
            {
                if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", "Starting Lobby Client Coroutine", "Multiplayer");
                StartCoroutine(LobbyClient());
            }
            if (CurrentPlayer.ReadOnlyTags().Contains(HostKey))
            {
                if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", "Starting Host Coroutine", "Multiplayer");
                StartCoroutine(Host());
            }
            if (CurrentPlayer.ReadOnlyTags().Contains(ClientKey))
            {
                if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", "Starting Client Coroutine", "Multiplayer");
                StartCoroutine(Client());
            }
            if (CurrentPlayer.ReadOnlyTags().Contains(ServerKey))
            {
                if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", "Starting Server Coroutine", "Multiplayer");
                StartCoroutine(Server());
            }
            yield break;
        }

        IEnumerator Server()
        {
            // Dedicated server is not supported by Multiplayer Services SDK
            Debug.Log("Dedicated server is not supported by Multiplayer Services SDK");
            yield break;
        }

        IEnumerator Client()
        {
            // Direct connection logic, outside of the Multiplayer Services SDK scope
            yield break;
        }

        IEnumerator Host()
        {
            // Direct connection logic, outside of the Multiplayer Services SDK scope
            yield break;
        }

        IEnumerator LobbyClient()
        {
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync(ProfileName);

            while (PlayerPrefs.HasKey("SessionCode") == false)
            {
                yield return null;
            }

            var sessionCode = PlayerPrefs.GetString("SessionCode");
            if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", $"Joining Session with Code {sessionCode}", "Multiplayer");
            SessionManager.Instance.JoinSessionById(sessionCode);
            yield break;
        }

        IEnumerator LobbyHost()
        {
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync(ProfileName);
            
            // Pass the session name here
            SessionManager.Instance.StartSessionAsHost(AuthenticationManager.Instance.PlayerName + "'s Game");
            
            while (SessionManager.Instance.ActiveSession == null)
            {
                yield return null;
            }

            var session = SessionManager.Instance.ActiveSession;
            PlayerPrefs.SetString("SessionCode", session.Id);
            if (_debugLog) Logwin.Log("MultiplayerPlayModeManager", $"Set SessionCode to {session.Id}", "Multiplayer");
        }

        void OnDestroy()
        {
            PlayerPrefs.DeleteKey("SessionCode");
        }
#endif
    }
}