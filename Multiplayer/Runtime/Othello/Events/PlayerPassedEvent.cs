namespace MidniteOilSoftware.Multiplayer.Othello
{
    public struct PlayerPassedEvent
    {
        public int ChipColor { get; private set; }
        
        public PlayerPassedEvent(int chipColor)
        {
            ChipColor = chipColor;
        }
    }
}