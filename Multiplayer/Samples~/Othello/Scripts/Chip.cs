using System.Collections;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class Chip : MonoBehaviour
    {
        public ChipColor Color { get; private set; }

        public void FlipChip()
        {
            Color = Color == ChipColor.Black ? ChipColor.White : ChipColor.Black;
            StartCoroutine(RotateChip());
        }
        
        public void SetColor(ChipColor color)
        {
            Color = color;
            float angle = color == ChipColor.Black ? 0 : 180;
            transform.rotation = Quaternion.Euler(angle, 0, 0);
        }
        
        IEnumerator RotateChip()
        {
            var startRotation = transform.rotation;
            var endRotation = startRotation * Quaternion.Euler(180, 0, 0);
            var t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime / 0.5f;
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }
        }
    }

    public enum ChipColor
    {
        White,
        Black
    }
}
