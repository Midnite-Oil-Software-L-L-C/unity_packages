using System.Collections;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameSessionCleanup : MonoBehaviour
    {
        public virtual IEnumerator CleanupSession()
        {
            // override this for any custom cleanup
            yield break;
        }
    }
}