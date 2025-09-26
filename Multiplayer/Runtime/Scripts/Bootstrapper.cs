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

        protected override async void Start()
        {
            Debug.Log("Bootstrapper Start");
            base.Start();
            Application.runInBackground = true;
            await UnityServices.InitializeAsync();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init()
        {
            if (SceneManager.GetSceneByName("Bootstrapper").IsValid() == true) return;
            Debug.Log("Loading Bootstrapper");
            await SceneManager.LoadSceneAsync("Bootstrapper", LoadSceneMode.Single);
        }
    }
}