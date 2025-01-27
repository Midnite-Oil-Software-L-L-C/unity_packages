namespace MidniteOilSoftware.Multiplayer.Events
{
    public struct LeftGameEvent
    {
        public string PlayerId { get; }
        public ulong ClientOwnerId { get; }

        public LeftGameEvent(string playerId, ulong clientOwnerId)
        {
            PlayerId = playerId;
            ClientOwnerId = clientOwnerId;
        }
    }
}
