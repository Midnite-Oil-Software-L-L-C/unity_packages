using System;
using MidniteOilSoftware.Core.Settings;
using MidniteOilSoftware.Multiplayer.Lobby;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer
{
    public class AuthenticationDialogBase : MonoBehaviour
    {
        public string AuthenticationMethodName => _authenticationMethodName.text; 
        public event Action<string> AuthenticationComplete;
        public event Action AuthenticationCanceled;

        [SerializeField] TMP_Text _authenticationMethodName;
        [SerializeField] protected TMP_Text _statusText;
        
        [Header("Input Fields")] 
        [SerializeField] protected TMP_InputField _usernameInput;
        
        [Header("Buttons")] 
        [SerializeField] protected Button _loginButton;
        [SerializeField] Button _cancelButton;

        const string UserNameKey = "username";

        void Start()
        {
            SetupAuthentication();
        }

        protected virtual void SetupAuthentication()
        {
            AuthenticationManager.Instance.OnSignedIn += HandlePlayerLoggedIn;
            AuthenticationManager.Instance.OnSigninFailed += HandleSigninFailed;
            _loginButton.onClick.AddListener(Login);
            _cancelButton.onClick.AddListener(CancelLogin);
            ShowLoginPanel();
        }

        void ShowLoginPanel()
        {
            var userName = SettingsManager.Instance.GetSetting(UserNameKey, "player");

#if UNITY_EDITOR
            userName = FindFirstObjectByType<MPPMManager>().ProfileName 
                       ?? userName;
#endif

            if (!string.IsNullOrWhiteSpace(userName))
            {
                _usernameInput.text = userName;
            }

            _loginButton.interactable = true;
            _statusText.SetText("");
        }

        protected virtual async void Login()
        {
        }

        protected void HandlePlayerLoggedIn()
        {
            // ShowLobbyPanel();
            // _playerIdText.SetText(AuthenticationService.Instance.PlayerId);
            SaveCredentials();
            AuthenticationComplete?.Invoke(AuthenticationManager.Instance.PlayerId);
        }

        void HandleSigninFailed(RequestFailedException e)
        {
            _statusText.SetText(e.Message);
            _loginButton.interactable = true;
        }

        protected virtual void SaveCredentials()
        {
            SettingsManager.Instance.SetSetting(UserNameKey, _usernameInput.text);
// #if UNITY_EDITOR
//             PlayerPrefs.SetString("password", _passwordInput.text);
// #endif
            PlayerPrefs.Save();            
        }
        
        void CancelLogin()
        {
            AuthenticationCanceled?.Invoke();
        }
    }
}
