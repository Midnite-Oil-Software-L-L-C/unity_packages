using MidniteOilSoftware.Core;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class TransportConfig : MonoBehaviour
    {
        [SerializeField] bool _debugLog;
        
        UnityTransport _unityTransport;

        void Awake()
        {
            _unityTransport = GetComponent<UnityTransport>();
            if (!_unityTransport)
            {
                if (_debugLog) Logwin.LogError("TransportConfig", 
                    "UnityTransport requires a UnityTransport component", "Multiplayer");
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
