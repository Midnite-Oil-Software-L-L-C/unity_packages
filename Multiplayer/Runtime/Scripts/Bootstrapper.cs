using MidniteOilSoftware.Core;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidniteOilSoftware.Multiplayer
{
    public class Bootstrapper : SingletonMonoBehaviour<Bootstrapper>
    {
        protected override void Awake()
        {
            Debug.Log("Bootstrapper Awake");
            base.Awake();
        }

        async void Start()
        {
            Debug.Log("Bootstrapper Start");
            Application.runInBackground = true;
            await UnityServices.InitializeAsync();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init()
        {
            if (SceneManager.GetSceneByName("Bootstrapper").IsValid() != true)
            {
                Debug.Log("Loading Bootstrapper");
                await SceneManager.LoadSceneAsync("Bootstrapper", LoadSceneMode.Single);
            }

            if (SceneManager.GetSceneByName("User Interface").IsValid() == true) return;
            Debug.Log("Loading UI scene additively");
            await SceneManager.LoadSceneAsync("User Interface", LoadSceneMode.Additive);
        }
    }
}