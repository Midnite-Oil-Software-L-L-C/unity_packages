using UnityEngine;
using UnityEngine.InputSystem;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class QuitOnEscape : MonoBehaviour
    {
        void Update()
        {
            if (Keyboard.current.escapeKey.isPressed)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        }
    }
}
