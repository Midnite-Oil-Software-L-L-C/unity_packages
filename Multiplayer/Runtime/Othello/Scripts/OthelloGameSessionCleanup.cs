using System.Collections;
using MidniteOilSoftware.Multiplayer;
using MidniteOilSoftware.Multiplayer.Lobby;

namespace MidniteOilSoftware.Core.Othello
{
    public class OthelloGameSessionCleanup : GameSessionCleanup
    {
        public override IEnumerator CleanupSession()
        {
            yield return base.CleanupSession();
            yield return ProjectSceneManager.Instance.UnloadCurrentScene();
        }
        
    }
}
