using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class LoginFormButton : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] TMP_Text _buttonText;
        
        public event Action<string> ButtonClicked;
        
        public void Initialize(string buttonText)
        {
            _buttonText.text = buttonText;
            _button.onClick.AddListener(HandleButtonClicked);
        }
        
        void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
        
        void HandleButtonClicked()
        {
            ButtonClicked?.Invoke(_buttonText.text);
        }
    }
}