namespace MidniteOilSoftware.Multiplayer.Events
{
    public struct GameStateChangedEvent
    {
        public GameState NewState;

        public GameStateChangedEvent(GameState newState)
        {
            NewState = newState;
        }
    }
}