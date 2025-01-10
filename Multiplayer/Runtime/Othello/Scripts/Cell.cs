using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class Cell : MonoBehaviour
    {
        public Chip Chip { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public void Initialize(int x, int y)
        {
            name = $"Cell ({x}, {y})";
            X = x;
            Y = y;
        }

        public void DropChip(Chip chip)
        {
            Chip = chip;
        }
        
        void OnDisable()
        {
            Chip = null;
        }
    }
}
