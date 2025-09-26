using System;
using System.Threading.Tasks;
using MidniteOilSoftware.Core;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace MidniteOilSoftware.Multiplayer.Authentication
{
    public class AuthenticationManager : SingletonMonoBehaviour<AuthenticationManager>
    {
        public event Action OnSignedIn;
        public event Action<RequestFailedException> OnSigninFailed;
        public string PlayerId => AuthenticationService.Instance.PlayerId;
        public string PlayerName { get; private set; }

        public ulong ConnectionId => NetworkManager.Singleton.LocalClientId;

        public async Task<SigninResult> SignInAnonymouslyAsync(string playerName)
        {
            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    AuthenticationService.Instance.SignOut();
                    await Task.Delay(1200); // Wait 1.2 seconds to avoid rate limiting
                }

                if (playerName == default && AuthenticationService.Instance.SessionTokenExists)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                else
                {
                    var options = new SignInOptions() { CreateAccount = true };
                    await AuthenticationService.Instance.SignInAnonymouslyAsync(options);
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName?.Replace(" ", ""));
                }

                PlayerName = playerName?.Replace(" ", "");
                return SigninResult.Successful;
            }
            catch (Exception ex)
            {
                return new SigninResult(false, ex.Message);
            }
        }

        public async Task<SigninResult> SignInWithUserNameAndPasswordAsync(string playerName, string password)
        {
            var trimmedPlayerName = playerName?.Replace(" ", "");
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(trimmedPlayerName, password);
                await AuthenticationService.Instance.UpdatePlayerNameAsync(trimmedPlayerName);

                PlayerName = trimmedPlayerName;
                return new SigninResult(true, String.Empty);
            }
            catch (Exception ex)
            {
                if (_enableDebugLog) Logwin.LogError("AuthenticationManager", ex.ToString(), "Multiplayer");
                return new SigninResult(false, ex.Message);
            }
        }

        public async Task<SigninResult> RegisterWithUserNameAndPasswordAsync(string playerName, string password)
        {
            var trimmedPlayerName = playerName?.Replace(" ", "");
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(trimmedPlayerName, password);
                PlayerName = trimmedPlayerName;
                return new SigninResult(true, String.Empty);
            }
            catch (Exception ex)
            {
                if (_enableDebugLog) Logwin.LogError("AuthenticationManager", ex.ToString(), "Multiplayer");
                return new SigninResult(false, ex.Message);
            }
        }

        protected override async void Start()
        {
            base.Start();
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
            OnSignedIn?.Invoke();
        }

        void HandleSignedOut()
        {
            if (_enableDebugLog) Logwin.Log("AuthenticationManager", "Signed Out", "Multiplayer");
        }

        void HandleSignInFailed(RequestFailedException ex)
        {
            if (_enableDebugLog) Logwin.LogError("AuthenticationManager", "Sign In failed: " + ex, "Multiplayer");
            OnSigninFailed?.Invoke(ex);
        }
    }
}