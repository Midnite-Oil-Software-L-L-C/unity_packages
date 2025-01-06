using TMPro;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.UI
{
    public class CenterGameName : MonoBehaviour
    {
        [SerializeField] TMP_Text _gameNameText;
        [SerializeField] RectTransform _parentRect, _renameGameButton;
        [SerializeField] float _spacing = 10f;

        void LateUpdate()
        {
            if (_gameNameText == null || _renameGameButton == null || _parentRect == null)
                return;

            var textWidth = _gameNameText.preferredWidth;

            _gameNameText.rectTransform.anchoredPosition =
                new Vector2(0, _gameNameText.rectTransform.anchoredPosition.y);

            var buttonWidth = _renameGameButton.rect.width;
            _renameGameButton.anchoredPosition = new Vector2(
                textWidth / 2 + _spacing + buttonWidth / 2,
                _renameGameButton.anchoredPosition.y
            );
        }
    }
}