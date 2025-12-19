using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidniteOilSoftware.Multiplayer
{
    /// <summary>
    /// Non-generic initializer for the Bootstrapper scene.
    /// Ensures the Bootstrapper scene loads before any other scene when the game starts.
    /// </summary>
    public static class BootstrapperInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init()
        {
            if (SceneManager.GetSceneByName("Bootstrapper").IsValid()) return;
            Debug.Log("Loading Bootstrapper");
            await SceneManager.LoadSceneAsync("Bootstrapper", LoadSceneMode.Single);
        }
    }
}