using System.Collections;
using MidniteOilSoftware.Core.Music;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MidniteOilSoftware.Core.Demo
{
    public class NavigationUI : MonoBehaviour
    {
        [SerializeField] string _music = "Menu Music", _nextScene = "Game";
 
        IEnumerator Start()
        {
            yield return null; // Wait a frame to ensure MusicManager is initialized
            EventBus.Instance.Raise(new PlayMusicEvent(_music)); 
        }

        void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(_nextScene);
            }
        
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
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