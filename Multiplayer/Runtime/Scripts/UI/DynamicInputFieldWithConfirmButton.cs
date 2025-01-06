using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class DynamicInputFieldWithConfirmButton : MonoBehaviour
    {
        public event Action EditCancelled;
        public event Action<string> EditConfirmed;

        public void Initialize(string text)
        {
            _inputField.text = text;
            _inputField.ActivateInputField();
        }

        [SerializeField] TMP_InputField _inputField;
        [SerializeField] Button _confirmButton;
        [SerializeField] float _minWidth = 100f;
        [SerializeField] float _maxWidth = 300f;
        [SerializeField] float _spacing = 10f;

        RectTransform _inputFieldRect, _buttonRect;

        void Start()
        {
            _inputFieldRect = _inputField.GetComponent<RectTransform>();
            _buttonRect = _confirmButton.GetComponent<RectTransform>();

            if (_inputField == null || _confirmButton == null || _inputFieldRect == null || _buttonRect == null)
            {
                Debug.LogError("Please assign all required components in the inspector.");
                return;
            }

            _confirmButton.onClick.AddListener(() => { EditConfirmed?.Invoke(_inputField.text); });
        }

        void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EditCancelled?.Invoke();
                return;
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                EditConfirmed?.Invoke(_inputField.text);
            }
        }

        void LateUpdate()
        {
            UpdateLayout();
        }

        void UpdateLayout()
        {
            if (_inputField == null || _confirmButton == null || _inputFieldRect == null)
                return;

            var textWidth = _inputField.textComponent.preferredWidth * 1.025f;

            var newWidth = Mathf.Clamp(textWidth, _minWidth, _maxWidth);
            _inputFieldRect.sizeDelta = new Vector2(newWidth, _inputFieldRect.sizeDelta.y);

            var buttonWidth = _buttonRect.rect.width;
            _buttonRect.anchoredPosition = new Vector2(
                _inputFieldRect.anchoredPosition.x + newWidth / 2 + _spacing + buttonWidth / 2,
                _buttonRect.anchoredPosition.y
            );
        }
    }
}