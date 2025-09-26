using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameSessionInitializer : MonoBehaviour
    {
        [SerializeField] protected int _maxPlayers = 4;
        [SerializeField] protected bool _enableDebugLog = true;
        
        public int MaxPlayers => _maxPlayers;
        
        public virtual void InitializeSession()
        {
            // override this for any custom initialization
        }
    }
}