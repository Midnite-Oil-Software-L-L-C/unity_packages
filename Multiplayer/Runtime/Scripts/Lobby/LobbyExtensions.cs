using System.Linq;
using Unity.Services.Lobbies.Models;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public static class LobbyExtensions
    {
        public static bool AllPlayersReady(this Unity.Services.Lobbies.Models.Lobby lobby)
        {
            try
            {
                return lobby?.Players?.All(player =>
                    player.Data.TryGetValue("isReady", out var ready) && ready.Value == "true") ?? false;
            }
            catch
            {
                return false;
            }
        }
    }
}
