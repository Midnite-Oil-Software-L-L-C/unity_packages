using UnityEngine;

namespace MidniteOilSoftware.Core
{
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            var resource = Resources.Load(@"--- Managers ---");
            if (resource == null) return;
            var managersObject = Object.Instantiate(resource);
            managersObject.name = @"--- Managers ---";
            Object.DontDestroyOnLoad(managersObject);
        }
    }
}