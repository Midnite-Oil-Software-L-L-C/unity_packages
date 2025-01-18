namespace MidniteOilSoftware.Multiplayer.Authentication
{
    public class AnonymousLoginDialog : AuthenticationDialogBase
    {
        protected override async void Login()
        {
            _loginButton.interactable = false;
            var result = await AuthenticationManager.Instance.SignInAnonymouslyAsync(_usernameInput.text);
            if (!result.Success)
            {
                _statusText.SetText(result.Message);
                _loginButton.interactable = true;
                return;
            }
            
            HandlePlayerLoggedIn();
        }
    }
}