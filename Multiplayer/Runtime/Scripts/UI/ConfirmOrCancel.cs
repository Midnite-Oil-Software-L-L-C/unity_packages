using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class ConfirmOrCancel : MonoBehaviour
    {
        [SerializeField] Button _confirmButton, _cancelButton;

        bool _formConfirmedOrCanceled;

        void Update()
        {
            if (_formConfirmedOrCanceled) return;
            if (Keyboard.current.enterKey.isPressed)
            {
                _formConfirmedOrCanceled = true;
                _confirmButton.onClick.Invoke();
                return;
            }

            if (!Keyboard.current.escapeKey.isPressed) return;

            _formConfirmedOrCanceled = true;
            _cancelButton.onClick.Invoke();
        }
    }
}