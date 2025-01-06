using Unity.Netcode;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public static class MultiplayerExtensionMethods
    {
        public static bool IsLocalPlayer(this ulong connectionId)
        {
            return connectionId == NetworkManager.Singleton.LocalClientId;
        }
    }
}