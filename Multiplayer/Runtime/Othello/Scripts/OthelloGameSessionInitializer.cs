using MidniteOilSoftware.Multiplayer;
using MidniteOilSoftware.Multiplayer.Lobby;

namespace MidniteOilSoftware.Core.Othello
{
    public class OthelloGameSessionInitializer : GameSessionInitializer
    {
        public override void InitializeSession()
        {
            base.InitializeSession();
            ProjectSceneManager.Instance.SetupSceneManagementAndLoadGameScene();
        }
    }
}
