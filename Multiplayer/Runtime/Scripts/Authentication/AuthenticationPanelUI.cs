using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.Authentication
{
    public class AuthenticationPanelUI : MonoBehaviour
    {
        [SerializeField] AuthenticationMethodsUI _authenticationMethodsPanel;
        [SerializeField] List<AuthenticationDialogBase> _authenticationDialogs;
        [SerializeField] Button _quitButton;
        
        public event Action<string, string> PlayerSignedIn;
        public event Action PlayerQuit;
        
        AuthenticationDialogBase _currentDialog;

        void Start()
        {
            #if UNITY_WEBGL
            _quitButton.gameObject.SetActive(false);
            #else
            _quitButton.onClick.RemoveAllListeners();
            _quitButton.onClick.AddListener(HandleQuitButtonClicked);
            #endif
            if (_authenticationDialogs.Count == 1)
            {
                HandleAuthenticationMethodSelected(_authenticationDialogs[0].AuthenticationMethodName);
                return;
            }
            _authenticationMethodsPanel.Initialize(_authenticationDialogs
                .Select(d => d.AuthenticationMethodName).ToList());
            _authenticationMethodsPanel.AuthenticationMethodSelected -= HandleAuthenticationMethodSelected;
            _authenticationMethodsPanel.AuthenticationMethodSelected += HandleAuthenticationMethodSelected;
            _authenticationMethodsPanel.gameObject.SetActive(true);
        }

        #if !UNITY_WEBGL
        void HandleQuitButtonClicked()
        {
            PlayerQuit?.Invoke();
        }
        #endif

        void HandleAuthenticationMethodSelected(string authenticationMethodName)
        {
            _currentDialog = GetAuthDialogFromName(authenticationMethodName);
            OpenAuthenticationDialog();
        }

        void HandlePlayerSignedIn(string playerId, string playerName)
        {
            CloseAuthenticationDialog();
            PlayerSignedIn?.Invoke(playerId, playerName);
        }

        void HandleLoginCanceled()
        {
            CloseAuthenticationDialog();
            if (_authenticationDialogs.Count == 1)
            {
                PlayerQuit?.Invoke(); 
                return;
            }
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
