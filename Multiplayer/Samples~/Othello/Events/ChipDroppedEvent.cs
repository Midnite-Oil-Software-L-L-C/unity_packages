namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class ChipDroppedEvent
    {
        public int ChipColor { get; private set; }
        
        public ChipDroppedEvent(int chipColor)
        {
            ChipColor = chipColor;
        }
    }
}
