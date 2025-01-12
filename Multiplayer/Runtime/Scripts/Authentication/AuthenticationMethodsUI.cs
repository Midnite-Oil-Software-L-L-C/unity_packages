using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class AuthenticationMethodsUI : MonoBehaviour
    {
        [SerializeField] Transform _buttonGroup;
        [SerializeField] LoginFormButton _loginButtonPrefab;
        
        public event Action<string> AuthenticationMethodSelected;

        List<string> _authenticationMethods;

        public void Initialize(List<string> authenticationMethods)
        {
            _authenticationMethods = authenticationMethods;
            foreach (var authenticationMethod in _authenticationMethods)
            {
                var button = Instantiate(_loginButtonPrefab, _buttonGroup);
                button.Initialize(authenticationMethod);
                button.ButtonClicked += HandleButtonClicked;
            }
        }   

        void HandleButtonClicked(string authenticationMethodName)
        {
            AuthenticationMethodSelected?.Invoke(authenticationMethodName);
        }


    }
}