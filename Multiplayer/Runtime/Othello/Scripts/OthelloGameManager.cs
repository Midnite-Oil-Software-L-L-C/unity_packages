using System.Collections.Generic;
using MidniteOilSoftware.Core;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Othello
{
    public class OthelloGameManager : GameManager
    {
        public List<NetworkPlayer> OthelloPlayers => Players;
        public OthelloPlayer CurrentOthelloPlayer => CurrentPlayer as OthelloPlayer;
        public OthelloPlayer LocalOthelloPlayer => LocalPlayer as OthelloPlayer;
        public NetworkList<int> PlayerChipColors { get; private set; } = new();
        public NetworkList<bool> PlayerPassed { get; private set; } = new();

        public ChipColor LocalPlayerChipColor
        {
            get
            {
                for (var i = 0; i < OthelloPlayers.Count; i++)
                {
                    if (OthelloPlayers[i] == LocalPlayer)
                    {
                        return (ChipColor)PlayerChipColors[i];
                    }
                } 
                return ChipColor.White;
            }
        }

        public bool IsLocalPlayerTurn
        {
            get
            {
                if (CurrentState != GameState.PlayerTurnStart) return false;
                return CurrentOthelloPlayer == LocalPlayer;
            }
        }

        OthelloBoard _board;
        OthelloBoard Board
        {
            get
            {
                if (_board == null)
                {
                    _board = FindFirstObjectByType<OthelloBoard>(FindObjectsInactive.Include);
                }
                return _board;
            }
        }

        protected override void Start()
        {
            base.Start();
            EventBus.Instance.Subscribe<ChipDroppedEvent>(ChipDroppedEventHandler);
        }

        protected override void JoinedGame(NetworkPlayer player)
        {
            base.JoinedGame(player);
            var reversiPlayer = player as OthelloPlayer;
            if (reversiPlayer == null)
            {
                Debug.LogError($"Player {player.name} is not a ReversiPlayer");
                return;
            }
        }

        protected override void ServerOnlyHandleGameStateChange()
        {
            Debug.Log($"ServerOnlyHandleGameStateChange. CurrentState = {CurrentState}");
            if (CurrentState == GameState.GameStarted)
            {
                SetPlayerChipColors();
            }
            base.ServerOnlyHandleGameStateChange();
        }

        protected override bool GameOver()
        {
            return Board.BoardIsFull;
        }

        void SetPlayerChipColors()
        {
            PlayerChipColors.Clear();
            var chipColor = (ChipColor)Random.Range(0, 1);
            _currentPlayerTurnIndex.Value = (int)chipColor;
            PlayerChipColors.Add((int)chipColor);
            PlayerChipColors.Add(chipColor == ChipColor.Black ? (int)ChipColor.White : (int)ChipColor.Black);
            Debug.Log($"Setting player chip colors. Player 0 = {(ChipColor)PlayerChipColors[0]} Player 1 = {(ChipColor)PlayerChipColors[1]}");
            Debug.Log($"_currentPlayerTurnIndex.Value = {_currentPlayerTurnIndex.Value}");
        }

        void ChipDroppedEventHandler(ChipDroppedEvent _)
        {
            SetGameState(GameState.PlayerTurnEnd);
        }

        void PlayerPassedEventHandler(PlayerPassedEvent e)
        {
            PlayerPassed[e.ChipColor] = true;
            var playersPassed = 0;
            foreach (var playerPassed in PlayerPassed)
            {
                if (playerPassed)
                {
                    playersPassed++;
                }
            }

            SetGameState(playersPassed == OthelloPlayers.Count 
                ? GameState.GameOver : GameState.PlayerTurnEnd);
        }
    }
}
