using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class Cell : MonoBehaviour
    {
        public Chip Chip { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        [SerializeField] Material _hoverMaterial, _illegalMoveMaterial;
        MeshRenderer _renderer;
        
        public void Initialize(int x, int y)
        {
            name = $"Cell ({x}, {y})";
            X = x;
            Y = y;
            _renderer = GetComponent<MeshRenderer>();
        }

        public void DropChip(Chip chip)
        {
            Chip = chip;
        }
        
        void OnDisable()
        {
            Chip = null;
        }

        public void Highlight(bool showHighlight, bool illegalMove = false)
        {
            _renderer.material = illegalMove ? _illegalMoveMaterial : _hoverMaterial;
            _renderer.enabled = showHighlight;
        }

        public void ClearChip()
        {
            if (!Chip) return;
            Destroy(Chip.gameObject);
            Chip = null;
        }
    }
}
