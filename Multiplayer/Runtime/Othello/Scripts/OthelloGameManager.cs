using System.Collections.Generic;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

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
                if (LocalPlayerIndex < PlayerChipColors.Count)
                {
                    return (ChipColor)PlayerChipColors[LocalPlayerIndex];
                }

                Debug.LogWarning(
                    $"LocalPlayerIndex {LocalPlayerIndex} out of range of PlayerChipColors.Count {PlayerChipColors.Count}");
                return ChipColor.White;
            }
        }

        public ChipColor CurrentPlayerChipColor => (ChipColor)PlayerChipColors[CurrentPlayerTurnIndex];

        public int LocalPlayerIndex => OthelloPlayers.IndexOf(LocalOthelloPlayer);

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

        void OnDisable()
        {
            if (!IsHost)
            {
                EventBus.Instance.Raise<LeftGameEvent>(new LeftGameEvent());
            }
        }

        protected override void JoinedGame(NetworkPlayer player)
        {
            base.JoinedGame(player);
            var reversiPlayer = player as OthelloPlayer;
            if (reversiPlayer == null)
            {
                Debug.LogError($"Player {player.name} is not a ReversiPlayer");
            }
        }

        protected override void ServerOnlyHandleGameStateChange()
        {
            Debug.Log($"ServerOnlyHandleGameStateChange. CurrentState = {CurrentState}");
            if (CurrentState == GameState.GameStarted)
            {
                InitializePlayerOrderAndChipColors();
            }
            base.ServerOnlyHandleGameStateChange();
        }

        protected override bool IsGameOver()
        {
            return Board.BoardIsFull;
        }

        void InitializePlayerOrderAndChipColors()
        {
            Debug.Log("Initializing player order and chip colors");
            PlayerChipColors.Clear();
            var chipColor = (ChipColor)Random.Range(0, 1);
            _currentPlayerTurnIndex.Value = (int)chipColor;
            PlayerChipColors.Add((int)chipColor);
            PlayerChipColors.Add(chipColor == ChipColor.Black ? (int)ChipColor.White : (int)ChipColor.Black);
            Debug.Log($"Setting player chip colors. Player 0 = {(ChipColor)PlayerChipColors[0]} Player 1 = {(ChipColor)PlayerChipColors[1]}");
            Debug.Log($"LocalPlayer({LocalPlayerIndex} ChipColor = {LocalPlayerChipColor}");
            Debug.Log($"_currentPlayerTurnIndex.Value = {_currentPlayerTurnIndex.Value}");
            PlayerPassed.Clear();
            PlayerPassed.Add(false);
            PlayerPassed.Add(false);
        }
        
        void ChipDroppedEventHandler(ChipDroppedEvent _)
        {
            ClearPlayerPassedServerRpc();
            SetGameState(GameState.PlayerTurnEnd);
        }
        
        [Rpc(SendTo.Server)]
        void ClearPlayerPassedServerRpc()
        {
            PlayerPassed[0] = PlayerPassed[1] = false;
        }

        [Rpc(SendTo.Server)]
        public void PlayerPassedServerRpc(ChipColor _)
        {
            PlayerPassed[CurrentPlayerTurnIndex] = true;
            var playersPassed = 0;
            foreach (var passed in PlayerPassed)
            {
                if (passed)
                {
                    playersPassed++;
                }
            }
            Debug.Log($"{CurrentPlayerChipColor} passed. {playersPassed}/{OthelloPlayers.Count} players have passed.");

            SetGameState(playersPassed == OthelloPlayers.Count 
                ? GameState.GameOver : GameState.PlayerTurnEnd);
        }

        [Rpc(SendTo.Server)]
        public void RematchServerRpc()
        {
            ClearPlayerPassedServerRpc();
            SetGameState(GameState.GameRestarted);
        }

        protected override void CleanupSession()
        {
            Debug.Log("Cleaning up OthelloGameManager session", this);
            ClearCollections();
            base.CleanupSession();
        }

        void ClearCollections()
        {
            PlayerChipColors.Clear();
            PlayerPassed.Clear();
        }

    }
}
