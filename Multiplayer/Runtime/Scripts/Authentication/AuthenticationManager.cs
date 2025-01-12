using System;
using System.Threading.Tasks;
using MidniteOilSoftware.Core;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class AuthenticationManager : SingletonMonoBehaviour<AuthenticationManager>
    {
        public event Action OnSignedIn;
        public event Action<RequestFailedException> OnSigninFailed;
        public string PlayerId => AuthenticationService.Instance.PlayerId;
        public string PlayerName => AuthenticationService.Instance.PlayerName;
        public ulong ConnectionId => NetworkManager.Singleton.LocalClientId;

        const float MessageLimitRate = 1.2f;
        PlayerInfo _playerInfo;

        public async Awaitable<SigninResult> SignInAnonymouslyAsync(string playerName)
        {
            try
            {
                Debug.Log("Signing In Anonymously");
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    AuthenticationService.Instance.SignOut();
                    await Awaitable.WaitForSecondsAsync(MessageLimitRate);
                }

                if (playerName == default && AuthenticationService.Instance.SessionTokenExists)
                {
                    Debug.Log("Session Token Exists");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                else
                {
                    Debug.Log("Signing In Anonymously");
                    SignInOptions options = new SignInOptions() { CreateAccount = true };
                    await AuthenticationService.Instance.SignInAnonymouslyAsync(options);
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                }

                return SigninResult.Successful;
            }
            catch (Exception ex)
            {
                return new SigninResult(false, ex.Message);
            }
        }

        public async Task<SigninResult> SignInWithUserNameAndPasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                await AuthenticationService.Instance.UpdatePlayerNameAsync(username);

                Debug.Log($"{username} Signed In");
                return new SigninResult(true, String.Empty);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return new SigninResult(false, ex.Message);
            }
        }

        public async Task<SigninResult> RegisterWithUserNameAndPasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                return new SigninResult(true, String.Empty);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return new SigninResult(false, ex.Message);
            }
        }

        async void Start()
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += HandleSignedInAnon;
            AuthenticationService.Instance.SignedOut += HandleSignedOut;
            AuthenticationService.Instance.SignInFailed += HandleSignInFailed;
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                AuthenticationService.Instance.SignedIn -= HandleSignedInAnon;
                AuthenticationService.Instance.SignedOut -= HandleSignedOut;
                AuthenticationService.Instance.SignInFailed -= HandleSignInFailed;
            }

            base.OnDestroy();
        }


        void HandleSignedInAnon()
        {
            Debug.Log("Signed In");
            OnSignedIn?.Invoke();
        }

        void HandleSignedOut()
        {
            Debug.Log("Signed Out");
        }

        void HandleSignInFailed(RequestFailedException obj)
        {
            Debug.Log($"Sign In Failed: {obj.Message}");
            OnSigninFailed?.Invoke(obj);
        }
    }
}