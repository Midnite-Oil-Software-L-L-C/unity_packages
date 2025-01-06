using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class CachedTabNavigation : MonoBehaviour
    {
        private EventSystem _eventSystem;
        private InputAction _tabAction;
        private List<Selectable> _selectables;
        int _currentIndex = 0;

        private void Awake()
        {
            _eventSystem = EventSystem.current;

            // Cache all selectables in the scene
            CacheInputFields();

            // Create InputAction for Tab
            CreateTabInputAction();
        }

        void OnEnable()
        {
            if (_selectables.Count > 0)
            {
                NavigateToSelectable(_selectables[_currentIndex]);
            }
        }

        private void OnDestroy()
        {
            CleanupTabInputAction();
        }

        void CreateTabInputAction()
        {
            _tabAction = new InputAction("Tab", binding: "<Keyboard>/tab");
            _tabAction.performed += OnTabPressed;
            _tabAction.Enable();
        }

        void CleanupTabInputAction()
        {
            _tabAction.performed -= OnTabPressed;
            _tabAction.Disable();
        }

        void CacheInputFields()
        {
            // Cache all Selectables in the scene
            _selectables = new List<Selectable>(FindObjectsByType<Selectable>(FindObjectsSortMode.None));
            _selectables.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        }

        void OnTabPressed(InputAction.CallbackContext context)
        {
            if (_selectables == null || _selectables.Count == 0) return;

            // Get the currently selected UI element
            var currentSelectable = _eventSystem.currentSelectedGameObject?.GetComponent<Selectable>();
            _currentIndex = _selectables.IndexOf(currentSelectable);

            // Determine direction (Shift for reverse)
            var isShiftPressed = Keyboard.current.shiftKey.isPressed;
            var nextIndex = isShiftPressed
                ? (_currentIndex <= 0 ? _selectables.Count - 1 : _currentIndex - 1)
                : (_currentIndex >= _selectables.Count - 1 ? 0 : _currentIndex + 1);

            // Get the next selectable UI element
            var nextSelectable = _selectables[nextIndex];
            NavigateToSelectable(nextSelectable);
        }

        void NavigateToSelectable(Selectable nextSelectable)
        {
            if (nextSelectable == null) return;

            // Check if the next selectable is an input field
            var inputField = nextSelectable.GetComponent<InputField>();
            var tmpInputField = nextSelectable.GetComponent<TMP_InputField>();

            if (inputField != null)
            {
                inputField.OnPointerClick(new PointerEventData(_eventSystem)); // Set caret for InputField
            }
            else if (tmpInputField != null)
            {
                tmpInputField.OnPointerClick(new PointerEventData(_eventSystem)); // Set caret for TMP_InputField
            }

            // Set the next selectable as the current selected object
            _eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
        }
    }
}
