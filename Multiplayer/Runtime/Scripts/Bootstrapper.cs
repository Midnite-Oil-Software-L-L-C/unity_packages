using MidniteOilSoftware.Core;
using Unity.Services.Core;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    /// <summary>
    /// Main bootstrapper for the multiplayer framework.
    /// Initializes Unity Gaming Services and persists across scene loads.
    /// The scene loading is handled by BootstrapperInitializer.
    /// </summary>
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
    }
}