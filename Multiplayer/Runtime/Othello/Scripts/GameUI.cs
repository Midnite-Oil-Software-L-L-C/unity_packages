using System.Collections;
using MidniteOilSoftware.Multiplayer.Events;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using EventBus = MidniteOilSoftware.Core.EventBus;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] TMP_Text[] _playerNames;
        [SerializeField] TMP_Text[] _playerScores;
        [SerializeField] Image[] _playerChips;
        [SerializeField] Sprite[] _chipSprites;
        [SerializeField] TMP_Text _winnerText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] RectTransform[] _playerDisplayAreas;
        [SerializeField] Button _exitButton;

        OthelloGameManager _gameManager;
        OthelloBoard _othelloBoard;
        Coroutine _currentPlayerAnimation;

        void Start()
        {
            _gameManager = FindFirstObjectByType<OthelloGameManager>(FindObjectsInactive.Include);
            _othelloBoard = FindFirstObjectByType<OthelloBoard>(FindObjectsInactive.Include);
            _gameOverPanel.SetActive(false);
            EventBus.Instance.Subscribe<GameStateChangedEvent>(GameStateChangedEventHandler);
            _gameManager.PlayerChipColors.OnListChanged += PlayerChipColorsChanged;
        }

        void PlayerChipColorsChanged(NetworkListEvent<int> _)
        {
            UpdatePlayerDisplay();
        }

        void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(GameStateChangedEventHandler);
            _gameManager.PlayerChipColors.OnListChanged -= PlayerChipColorsChanged;
        }

        void GameStateChangedEventHandler(GameStateChangedEvent e)
        {
            Debug.Log($"GameUI. GameStateChangedEventHandler: {e.NewState}", this);
            switch (e.NewState)
            {
                case GameState.PlayerTurnStart:
                    UpdatePlayerDisplay();
                    StartCurrentPlayerAnimation();
                    break;
                case GameState.PlayerTurnEnd:
                    StopCurrentPlayerAnimation();
                    UpdatePlayerDisplay();
                    break;
                case GameState.GameOver:
                    ShowGameOverPanel();
                    break;
            }
        }

        void StartCurrentPlayerAnimation()
        {
            var currentPlayerArea = _playerDisplayAreas[_gameManager.CurrentPlayerTurnIndex];
            _currentPlayerAnimation = StartCoroutine(AnimateCurrentPlayerArea(currentPlayerArea));
        }

        IEnumerator AnimateCurrentPlayerArea(RectTransform currentPlayerArea)
        {
            var startScale = currentPlayerArea.localScale;
            var endScale = startScale * 1.5f;
            var t = 0f;
            var duration = 0.5f;

            while (true)
            {
                // Scale up
                while (t < 1)
                {
                    t += Time.deltaTime / duration;
                    currentPlayerArea.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }

                // Reset t and swap scales for yoyo effect
                t = 0f;
                (startScale, endScale) = (endScale, startScale);
            }
        }

        void StopCurrentPlayerAnimation()
        {
            if (_currentPlayerAnimation != null)
            {
                StopCoroutine(_currentPlayerAnimation);
            }
            foreach (var playerArea in _playerDisplayAreas)
            {
                playerArea.localScale = Vector3.one;
            }
            _currentPlayerAnimation = null;
        }

        void ShowGameOverPanel()
        {
            _gameOverPanel.SetActive(true);
            _winnerText.text = _othelloBoard.BlackChips > _othelloBoard.WhiteChips ? "Black Wins!" : "White Wins!";
        }

        void UpdatePlayerDisplay()
        {
            for (var i = 0; i < _gameManager.OthelloPlayers.Count; i++)
            {
                var player = _gameManager.OthelloPlayers[i] as OthelloPlayer;
                if (player == null)
                {
                    _playerNames[i].text = "<no name>";
                    continue;
                }
                var playerChipColors = _gameManager.PlayerChipColors;
                if (playerChipColors is not { Count: 2 })
                {
                    Debug.Log("PlayerChipColors is null or not the correct length");
                    continue;
                }
                var chipColor = (ChipColor)playerChipColors[i];
                _playerNames[i].text = player.PlayerName.Value.ToString();
                _playerScores[i].text = chipColor == ChipColor.Black
                    ? _othelloBoard.BlackChips.ToString()
                    : _othelloBoard.WhiteChips.ToString();
                _playerChips[i].sprite = _chipSprites[(int)chipColor];
            }
        }
    }
}
