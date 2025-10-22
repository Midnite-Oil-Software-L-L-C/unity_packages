using MidniteOilSoftware.Multiplayer;
using MidniteOilSoftware.Multiplayer.Lobby;
using UnityEngine;

namespace MidniteOilSoftware.Core.Othello
{
    public class OthelloGameSessionInitializer : GameSessionInitializer
    {
        public override void InitializeSession()
        {
            base.InitializeSession();
            if (_enableDebugLog)
                Logwin.Log("OthelloGameSessionInitializer", "Initializing Othello game session...", "Multiplayer");
            ProjectSceneManager.Instance.SetupSceneManagementAndLoadGameScene();
        }
    }
}
