using System;
using System.Collections;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class MPPMManager : MonoBehaviour
    {
#if UNITY_EDITOR
        public string ProfileName { get; private set; }

        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            yield return InitializeMppm();
        }

        const string HOST = "Host";
        const string CLIENT = "Client";
        const string SERVER = "Server";
        const string LOBBY_HOST = "Lobby Host";
        const string LOBBY_CLIENT = "Lobby Client";

        IEnumerator InitializeMppm()
        {
            Debug.Log("InitializeMPPM");
            ProfileName = CurrentPlayer.ReadOnlyTags().Except(new[]
                {
                    LOBBY_HOST, LOBBY_CLIENT, HOST, CLIENT, SERVER
                })
                .FirstOrDefault();

            Debug.Log("ProfileName " + ProfileName);
            if (ProfileName != default)
            {
                AuthenticationService.Instance.SwitchProfile(ProfileName);
            }

            Debug.Log($"Starting as {ProfileName}");


            if (CurrentPlayer.ReadOnlyTags().Contains(LOBBY_HOST))
                yield return LobbyHost();
            if (CurrentPlayer.ReadOnlyTags().Contains(LOBBY_CLIENT))
                yield return LobbyClient();
            if (CurrentPlayer.ReadOnlyTags().Contains(HOST))
                yield return Host();
            if (CurrentPlayer.ReadOnlyTags().Contains(CLIENT))
                yield return Client();
            if (CurrentPlayer.ReadOnlyTags().Contains(SERVER))
                Server();
        }

        static void Server()
        {
            // NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
            NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
            NetworkManager.Singleton.StartServer();
        }

        IEnumerator Client()
        {
            LobbyManager.Instance.gameObject.SetActive(false);
            Debug.Log("Signing in Client " + ProfileName);
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync("Client" + ProfileName);

            Debug.Log("Start Client");

            var transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            if (transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
                transport.SetConnectionData("127.0.0.1", 7777);



            yield return PlayerConnectionsManager.Instance.StartClient();
            NetworkManager.Singleton.OnClientDisconnectCallback += args =>
            {
                Debug.LogError($"Client Disconnected {args}");
                Debug.LogError(NetworkManager.Singleton.DisconnectReason);
            };
        }

        IEnumerator Host()
        {
            LobbyManager.Instance.gameObject.SetActive(false);
            Debug.Log("Starting Host");
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync("Host" + ProfileName);

            NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);

            yield return PlayerConnectionsManager.Instance.StartHostOnServer();
        }

        IEnumerator LobbyClient()
        {
            yield return SignInAnon();

            while (PlayerPrefs.HasKey("LobbyId") == false)
            {
                yield return null;
            }

            var lobbyId = PlayerPrefs.GetString("LobbyId");
            Debug.Log($"Joining Lobby {lobbyId}");
            LobbyManager.Instance.JoinLobbyById(lobbyId);
        }

        async Awaitable SignInAnon()
        {
            try
            {
                AuthenticationService.Instance.SwitchProfile(ProfileName);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            Debug.Log("Signing in");

            SignInOptions options = new SignInOptions() { CreateAccount = true };
            await AuthenticationService.Instance.SignInAnonymouslyAsync(options);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(ProfileName);
            Debug.Log($"Set Name to {ProfileName}");
        }

        IEnumerator LobbyHost()
        {
            yield return AuthenticationManager.Instance.SignInAnonymouslyAsync("Host" + ProfileName);
            yield return new WaitUntil(() => AuthenticationService.Instance.IsSignedIn);

            var lobbyTask = LobbyManager.Instance.HostLobby();
            yield return new WaitUntil(() => lobbyTask.IsCompleted);

            try
            {
                var lobby = lobbyTask.Result;
                PlayerPrefs.SetString("LobbyId", lobby.Id);
                Debug.Log($"Set LobbyId to {lobby.Id}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        void OnDestroy()
        {
            PlayerPrefs.DeleteKey("LobbyId");
        }
#endif
    }
}