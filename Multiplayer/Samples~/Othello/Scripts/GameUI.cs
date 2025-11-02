using System.Collections;
using MidniteOilSoftware.Multiplayer.Events;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EventBus = MidniteOilSoftware.Core.EventBus;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] TMP_Text[] _playerNames, _playerScores;
        [SerializeField] TMP_Text[] _playerPassedText;
        [SerializeField] Image[] _playerChips;
        [SerializeField] Sprite[] _chipSprites;
        [SerializeField] TMP_Text _getReadyText, _winnerText;
        [SerializeField] GameObject _gameOverPanel;
        [SerializeField] RectTransform[] _playerDisplayAreas;
        [SerializeField] Button _rematchButton, _exitButton;
        [SerializeField] Button[] _passButtons, _resignButtons;
        [SerializeField] GameObject _quantumConsole;

        OthelloGameManager _gameManager;
        OthelloBoard _othelloBoard;
        Coroutine _currentPlayerAnimation;
        int LocalPlayerIndex => (int)_gameManager.LocalOthelloPlayer.ConnectionId;

        void Start()
        {
            _getReadyText.gameObject.SetActive(true);
            _gameManager = FindFirstObjectByType<OthelloGameManager>(FindObjectsInactive.Include);
            _othelloBoard = FindFirstObjectByType<OthelloBoard>(FindObjectsInactive.Include);
            _gameOverPanel.SetActive(false);
            _quantumConsole.SetActive(false);
            SubscribeToEvents();
            AddButtonListeners();
        }

        void OnDisable()
        {
            Debug.Log("GameUI.OnDisable()", this);
            UnsubscribeFromEvents();
            RemoveButtonListeners();
        }

        void Update()
        {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                _quantumConsole.SetActive(!_quantumConsole.activeSelf);
            }
        }

        void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<GameStateChangedEvent>(GameStateChangedEventHandler);
            _gameManager.PlayerChipColors.OnListChanged += PlayerChipColorsChanged;
            _gameManager.PlayerPassed.OnListChanged += UpdatePassedText;
        }

        void UnsubscribeFromEvents()
        {
            EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(GameStateChangedEventHandler);
            if (!_gameManager) return;
            _gameManager.PlayerChipColors.OnListChanged -= PlayerChipColorsChanged;
            _gameManager.PlayerPassed.OnListChanged -= UpdatePassedText;
        }

        void AddButtonListeners()
        {
            RemoveButtonListeners();
            _rematchButton.onClick.AddListener(_gameManager.RematchServerRpc);
            _exitButton.onClick.AddListener(ExitGame);
            _passButtons[0].onClick.AddListener(PlayerPassed);
            _passButtons[1].onClick.AddListener(PlayerPassed);
            _resignButtons[0].onClick.AddListener(PlayerResigned);
            _resignButtons[1].onClick.AddListener(PlayerResigned);
        }

        void RemoveButtonListeners()
        {
            _rematchButton.onClick.RemoveAllListeners();
            _exitButton.onClick.RemoveAllListeners();
            _passButtons[0].onClick.RemoveAllListeners();
            _passButtons[1].onClick.RemoveAllListeners();
            _resignButtons[0].onClick.RemoveAllListeners();
            _resignButtons[1].onClick.RemoveAllListeners();
        }

        void PlayerResigned()
        {
            _resignButtons[_gameManager.CurrentPlayerTurnIndex].gameObject.SetActive(false);
            _passButtons[_gameManager.CurrentPlayerTurnIndex].gameObject.SetActive(false);
            _gameManager.PlayerResignedServerRpc(_gameManager.LocalOthelloPlayer.ConnectionId,
                _gameManager.LocalOthelloPlayer.PlayerName.Value.ToString());
        }

        void PlayerPassed()
        {
            _passButtons[_gameManager.CurrentPlayerTurnIndex].gameObject.SetActive(false);
            _gameManager.PlayerPassedServerRpc(_gameManager.LocalPlayerChipColor);
        }
        
        void ExitGame()
        {
            Debug.Log("GameUI.ExitGame()", this);
            if (_currentPlayerAnimation != null)
            {
                StopCurrentPlayerAnimation();
            }
            _gameManager.ExitGameServerRpc();
        }

        void PlayerChipColorsChanged(NetworkListEvent<int> _)
        {
            UpdatePlayerDisplay();
        }

        void GameStateChangedEventHandler(GameStateChangedEvent e)
        {
            Debug.Log($"GameUI. GameStateChangedEventHandler: {e.NewState}", this);
            switch (e.NewState)
            {
                case GameState.GameStarted:
                case GameState.GameRestarted:
                    _gameOverPanel.gameObject.SetActive(false);
                    _othelloBoard.SetupBoardForStartOfGame();
                    DisablePlayerPassControls();
                    break;
                case GameState.PlayerTurnStart:
                    UpdatePlayerDisplay();
                    StartCurrentPlayerAnimation();
                    break;
                case GameState.PlayerTurnEnd:
                    StopCurrentPlayerAnimation();
                    UpdatePlayerDisplay();
                    break;
                case GameState.GameOver:
                    UpdatePlayerDisplay();
                    ShowGameOverPanel();
                    break;
            }
        }

        #region Current player animation
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
            const float duration = 0.5f;

            while (true)
            {
                // Scale up
                while (t < 1)
                {
                    t += Time.deltaTime / duration;
                    currentPlayerArea.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }

                // Reset t and swap scales for yo-yo effect
                t = 0f;
                (startScale, endScale) = (endScale, startScale);
                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
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
        #endregion Current player animation

        int LocalPlayerChips => _gameManager.LocalPlayerChipColor == ChipColor.Black
            ? _othelloBoard.BlackChips
            : _othelloBoard.WhiteChips;

        int OpponentChips => _gameManager.LocalPlayerChipColor == ChipColor.Black
            ? _othelloBoard.WhiteChips
            : _othelloBoard.BlackChips;

        void ShowResignButton()
        {
            for(var i = 0; i < _resignButtons.Length; i++)
            {
                var showButton = _gameManager.LocalPlayerIndex == i &&
                                 _gameManager.CurrentOthelloPlayer == _gameManager.LocalOthelloPlayer &&
                                 LocalPlayerChips < OpponentChips;
                                 
                _resignButtons[i].gameObject.SetActive(showButton);
            }
        }

        void ShowPassButton()
        {
            var showPassButton = _gameManager.CurrentOthelloPlayer == _gameManager.LocalOthelloPlayer &&
                                  !_othelloBoard.HasLegalMove(_gameManager.LocalPlayerChipColor);

            try
            {
                for (var i = 0; i < _gameManager.OthelloPlayers.Count; i++)
                {
                    _passButtons[i].gameObject.SetActive(i == LocalPlayerIndex && showPassButton);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{e.Message}. _passButtons.Length = {_passButtons.Length}, players: {_gameManager.OthelloPlayers.Count}");
            }
        }

        void UpdatePassedText(NetworkListEvent<bool> e)
        {
            if (_gameManager.PlayerPassed.Count < 2)
            {
                _playerPassedText[0].gameObject.SetActive(false);
                _playerPassedText[1].gameObject.SetActive(false);
                return;
            }
            Debug.Log($"UpdatePlayerPassedText: {e.Type}({e.Value}). ({_gameManager.PlayerPassed[0]},{_gameManager.PlayerPassed[1]})");
            for (var i = 0; i < _gameManager.PlayerPassed.Count; i++)
            {
                _playerPassedText[i].gameObject.SetActive(_gameManager.PlayerPassed[i]);
            }
        }

        void DisablePlayerPassControls()
        {
            foreach (var passButton in _passButtons)
            {
                passButton.gameObject.SetActive(false);
            }
            foreach (var passedText in _playerPassedText)
            {
                passedText.gameObject.SetActive(false);
            }
        }

        void ShowGameOverPanel()
        {
            _gameOverPanel.SetActive(true);
            _winnerText.text = _othelloBoard.BlackChips > _othelloBoard.WhiteChips ? "Black Wins!" : "White Wins!";
        }

        void UpdatePlayerDisplay()
        {
            if (_gameManager.PlayerChipColors is not { Count: 2 }) return;
            _getReadyText.gameObject.SetActive(false);
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
                    continue;
                }
                var chipColor = (ChipColor)playerChipColors[i];
                _playerNames[i].text = player.PlayerName.Value.ToString();
                _playerScores[i].text = chipColor == ChipColor.Black
                    ? _othelloBoard.BlackChips.ToString()
                    : _othelloBoard.WhiteChips.ToString();
                _playerChips[i].sprite = _chipSprites[(int)chipColor];
            }

            if (!_gameManager.IsPlaying.Value) return;
            ShowPassButton();
            ShowResignButton();
        }
    }
}
