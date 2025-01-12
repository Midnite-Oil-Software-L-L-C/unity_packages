using System;
using System.Collections.Generic;
using System.Linq;
using MidniteOilSoftware.Multiplayer.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer
{
    public class AuthenticationPanelUI : MonoBehaviour
    {
        [SerializeField] AuthenticationMethodsUI _authenticationMethodsPanel;
        [SerializeField] List<AuthenticationDialogBase> _authenticationDialogs;
        [SerializeField] Button _quitButton;
        
        public event Action<string> PlayerSignedIn;
        public event Action PlayerQuit;
        
        AuthenticationDialogBase _currentDialog;

        void Start()
        {
            _authenticationMethodsPanel.Initialize(_authenticationDialogs
                .Select(d => d.AuthenticationMethodName).ToList());
            _authenticationMethodsPanel.AuthenticationMethodSelected += HandleAuthenticationMethodSelected;
            _authenticationMethodsPanel.gameObject.SetActive(true);
            _quitButton.onClick.AddListener(HandleQuitButtonClicked);
        }

        void HandleQuitButtonClicked()
        {
            PlayerQuit?.Invoke();
        }

        void HandleAuthenticationMethodSelected(string authenticationMethodName)
        {
            _currentDialog = GetAuthDialogFromName(authenticationMethodName);
            OpenAuthenticationDialog();
        }

        void HandlePlayerSignedIn(string playerName)
        {
            CloseAuthenticationDialog();
            PlayerSignedIn?.Invoke(playerName);
        }

        void HandleLoginCanceled()
        {
            CloseAuthenticationDialog();
            _authenticationMethodsPanel.gameObject.SetActive(true);
        }

        void OpenAuthenticationDialog()
        {
            _authenticationMethodsPanel.gameObject.SetActive(false);
            _currentDialog.AuthenticationComplete += HandlePlayerSignedIn;
            _currentDialog.AuthenticationCanceled += HandleLoginCanceled;
            _currentDialog.gameObject.SetActive(true);
        }

        void CloseAuthenticationDialog()
        {
            _currentDialog.AuthenticationComplete -= HandlePlayerSignedIn;
            _currentDialog.AuthenticationCanceled -= HandleLoginCanceled;
            _currentDialog.gameObject.SetActive(false);
        }

        AuthenticationDialogBase GetAuthDialogFromName(string authMethodName)
        {
            return _authenticationDialogs
                .FirstOrDefault(dialog => dialog.AuthenticationMethodName == authMethodName);
        }

    }
}
