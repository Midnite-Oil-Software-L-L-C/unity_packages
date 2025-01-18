namespace MidniteOilSoftware.Multiplayer.Events
{
    public struct GameStateChangedEvent
    {
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState newState)
        {
            NewState = newState;
        }
    }
}