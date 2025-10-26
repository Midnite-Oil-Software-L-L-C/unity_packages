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
                var localPlayerIndex = LocalPlayerIndex;
                if (localPlayerIndex >= 0 && localPlayerIndex < PlayerChipColors.Count)
                {
                    return (ChipColor)PlayerChipColors[localPlayerIndex];
                }

                if (_enableDebugLog)
                {
                    Logwin.LogWarning("OthelloGameManager",
                        $"LocalPlayerIndex {localPlayerIndex} out of range of PlayerChipColors.Count {PlayerChipColors.Count}. Using default White.",
                        "Multiplayer");
                }
                return ChipColor.White;
            }
        }

        public ChipColor CurrentPlayerChipColor => 
            CurrentPlayerTurnIndex >= 0 && CurrentPlayerTurnIndex < PlayerChipColors.Count 
                ? (ChipColor)PlayerChipColors[CurrentPlayerTurnIndex] 
                : ChipColor.White;

        public int LocalPlayerIndex
        {
            get
            {
                if (LocalOthelloPlayer == null)
                {
                    if (_enableDebugLog)
                        Logwin.LogWarning("OthelloGameManager", "LocalOthelloPlayer is null", "Multiplayer");
                    return -1;
                }
                
                var index = OthelloPlayers.IndexOf(LocalOthelloPlayer);
                if (index == -1 && _enableDebugLog)
                {
                    Logwin.LogWarning("OthelloGameManager", 
                        $"LocalOthelloPlayer {LocalOthelloPlayer.PlayerName.Value} not found in OthelloPlayers list", 
                        "Multiplayer");
                }
                return index;
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
                if (!_board)
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
                EventBus.Instance?.Raise<LeftGameEvent>(new LeftGameEvent());
            }
        }

        protected override void JoinedGame(NetworkPlayer player)
        {
            base.JoinedGame(player);
            var othelloPlayer = player as OthelloPlayer;
            if (!othelloPlayer)
            {
                Debug.LogError($"Player {player.name} is not a OthelloPlayer");
            }
        }

        protected override void ServerOnlyHandleGameStateChange()
        {
            if (_enableDebugLog)
                Logwin.Log("OthelloGameManager", $"ServerOnlyHandleGameStateChange. CurrentState = {CurrentState}", "Multiplayer");
            
            if (CurrentState == GameState.GameStarted)
            {
                // Ensure we have players before initializing
                if (Players.Count >= 2)
                {
                    InitializePlayerOrderAndChipColors();
                }
                else
                {
                    if (_enableDebugLog)
                        Logwin.LogError("OthelloGameManager", 
                            $"Not enough players to start game. Current: {Players.Count}, Required: 2", 
                            "Multiplayer");
                    return;
                }
            }
            base.ServerOnlyHandleGameStateChange();
        }

        protected override bool IsGameOver()
        {
            return Board.BoardIsFull;
        }

        void InitializePlayerOrderAndChipColors()
        {
            if (_enableDebugLog)
                Logwin.Log("OthelloGameManager", "Initializing player order and chip colors", "Multiplayer");
            
            PlayerChipColors.Clear();
            var chipColor = (ChipColor)Random.Range(0, 2);
            _currentPlayerTurnIndex.Value = (int)chipColor;
            PlayerChipColors.Add((int)chipColor);
            PlayerChipColors.Add(chipColor == ChipColor.Black ? (int)ChipColor.White : (int)ChipColor.Black);
            
            if (_enableDebugLog)
            {
                Logwin.Log("OthelloGameManager", 
                    $"Setting player chip colors. Player 0 = {(ChipColor)PlayerChipColors[0]} Player 1 = {(ChipColor)PlayerChipColors[1]}", 
                    "Multiplayer");
                Logwin.Log("OthelloGameManager", 
                    $"LocalPlayer({LocalPlayerIndex}) ChipColor = {LocalPlayerChipColor}", 
                    "Multiplayer");
                Logwin.Log("OthelloGameManager", 
                    $"_currentPlayerTurnIndex.Value = {_currentPlayerTurnIndex.Value}", 
                    "Multiplayer");
            }
            
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
            
            if (_enableDebugLog)
                Logwin.Log("OthelloGameManager", 
                    $"{CurrentPlayerChipColor} passed. {playersPassed}/{OthelloPlayers.Count} players have passed.", 
                    "Multiplayer");

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
            if (_enableDebugLog)
                Logwin.Log("OthelloGameManager", "Cleaning up OthelloGameManager session", "Multiplayer");
            
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
