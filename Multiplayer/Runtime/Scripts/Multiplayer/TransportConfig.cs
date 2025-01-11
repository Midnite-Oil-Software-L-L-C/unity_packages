using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class TransportConfig : MonoBehaviour
    {
        UnityTransport _unityTransport;

        void Awake()
        {
            _unityTransport = GetComponent<UnityTransport>();
            if (_unityTransport == null)
            {
                Debug.LogError("UnityTransport requires a UnityTransport component");
                return;
            }
            #if UNITY_WEBGL
                _unityTransport.UseWebSockets = true;
            #else
                _unityTransport.UseWebSockets = false;
            #endif
        }
    }
}
