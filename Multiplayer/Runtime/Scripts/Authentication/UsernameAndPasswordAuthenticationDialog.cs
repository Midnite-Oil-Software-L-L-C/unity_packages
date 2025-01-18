using MidniteOilSoftware.Core.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Authentication
{
    public class UsernameAndPasswordAuthenticationDialog : AuthenticationDialogBase
    {
        [Header("Input Fields")] 
        [SerializeField] TMP_InputField _passwordInput;
        
        [Header("Buttons")] 
        [SerializeField] Button _registerButton;

        const string PasswordKey = "password";
        
        protected override void SetupAuthentication()
        {
            _registerButton.onClick.AddListener(Register);
            base.SetupAuthentication();
        }

        protected override async void Login()
        {
            _loginButton.interactable = false;
            var result =
                await AuthenticationManager.Instance.SignInWithUserNameAndPasswordAsync(
                    _usernameInput.text, _passwordInput.text);
            if (!result.Success)
            {
                _statusText.SetText(result.Message);
                _loginButton.interactable = true;
                return;
            }
            
            HandlePlayerLoggedIn();
        }

        async void Register()
        {
            _registerButton.interactable = false;

            var result = await AuthenticationManager.Instance.RegisterWithUserNameAndPasswordAsync(
                _usernameInput.text, _passwordInput.text);
            if (!result.Success)
            {
                _registerButton.interactable = true;
                _statusText.SetText(result.Message);
                return;
            }

            HandlePlayerLoggedIn();
        }

        protected override void SaveCredentials()
        {
            SettingsManager.Instance.SetSetting(PasswordKey, _passwordInput.text);
            base.SaveCredentials();
        }
    }
}