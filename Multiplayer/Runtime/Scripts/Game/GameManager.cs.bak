using System.Collections.Generic;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] protected bool _enableDebugLog = true;
        
        public GameState CurrentState { get; private set; } = GameState.None;
        public int CurrentPlayerTurnIndex => _currentPlayerTurnIndex.Value;
        public NetworkPlayer CurrentPlayer
        {
            get
            {
                if (CurrentPlayerTurnIndex < Players.Count) return Players[CurrentPlayerTurnIndex];
                if (_enableDebugLog)
                {
                    Logwin.LogError("GameManager",
                        $"CurrentPlayerTurnIndex {CurrentPlayerTurnIndex} out of range of {Players.Count} players",
                        "Multiplayer");
                }
                return null;
            }
        }

        public NetworkPlayer LocalPlayer { get; private set; }
        
        public NetworkVariable<bool> IsPlaying { get; private set; } = new();

        protected List<NetworkPlayer> Players { get; set; } = new();
        protected NetworkVariable<int> _currentPlayerTurnIndex = new();

        protected virtual void Start()
        {
            // Remove the direct NetworkManager callback - PlayerRegistry handles this now
            EventBus.Instance.Subscribe<GameStartedEvent>(HandleGameStarted);
            SetGameState(GameState.WaitingForPlayers);
        }

        GameState _previousState, _newState;
        protected void SetGameState(GameState newState, float delay = 0f)
        {
            if (!IsHost) return;
            _previousState = CurrentState;
            _newState = newState;
            if (delay > 0)
            {
                Invoke(nameof(TransitionToNewState), delay);
                return;
            }
            TransitionToNewState();
        }

        void TransitionToNewState()
        {
            CurrentState = _newState;
            SetGameStateOnClientRpc(_newState);
            OnGameStateChanged(_previousState, _newState);
        }

        [Rpc(SendTo.NotServer)]
        void SetGameStateOnClientRpc(GameState newState)
        {
            var previousState = CurrentState;
            CurrentState = newState;
            OnGameStateChanged(previousState, newState);
        }

        protected virtual void OnGameStateChanged(GameState previousState, GameState newState)
        {
            if (_enableDebugLog)
            {
                Logwin.Log(
                    "GameManager",
                    $"GameState changed from {previousState} to {newState} IsServer = {IsServer} IsClient = {IsClient} IsHost = {IsHost}",
                    "Multiplayer");
            }

            if (IsServer)
            {
                ServerOnlyHandleGameStateChange();
            }
            else
            {
                ClientOnlyHandleGameStateChange();
            }

            if (_enableDebugLog)
            {
                Logwin.Log("GameManager", $"Raising GameStateChangedEvent({newState})", "Multiplayer");
            }
            EventBus.Instance.Raise(new GameStateChangedEvent(newState));
        }

        protected virtual void ServerOnlyHandleGameStateChange()
        {
            switch (CurrentState)
            {
                case GameState.WaitingForPlayers:
                    // PlayerRegistry handles player tracking automatically
                    IsPlaying.Value = false;
                    break;
                case GameState.GameStarted:
                    _currentPlayerTurnIndex.Value = 0;
                    IsPlaying.Value = true;
                    SetGameState(GameState.PlayerTurnStart, 0.25f);
                    break;
                case GameState.GameRestarted:
                    IsPlaying.Value = true;
                    SetGameState(GameState.PlayerTurnStart, 0.25f);
                    break;
                case GameState.PlayerTurnEnd:
                    if (IsGameOver())
                    {
                        IsPlaying.Value = false;
                        SetGameState(GameState.GameOver, 0.25f);
                        return;
                    }
                    _currentPlayerTurnIndex.Value = (CurrentPlayerTurnIndex + 1) % Players.Count;
                    SetGameState(GameState.PlayerTurnStart, 0.25f);
                    break;                
            }
        }
        
        protected virtual void ClientOnlyHandleGameStateChange()
        {
            switch (CurrentState)
            {
                case GameState.GameStarted:
                    // Get players from PlayerRegistry instead of direct NetworkManager access
                    GetPlayersFromRegistry();
                    break;
            }
        }

        protected virtual bool IsGameOver()
        {
            throw new System.NotImplementedException("You must override this method in a subclass");
        }

        protected virtual void JoinedGame(NetworkPlayer player)
        {
            if (_enableDebugLog)
                Logwin.Log("GameManager", 
                    $"Player joined game: {player.PlayerName.Value}", "Multiplayer");
        }
        
        void GetPlayersFromRegistry()
        {
            if (IsHost) return; // Host already has players from HandleGameStarted

            if (PlayerRegistry.Instance)
            {
                Players = PlayerRegistry.Instance.GetPlayers();
                LocalPlayer = PlayerRegistry.Instance.LocalPlayer;
                
                if (_enableDebugLog)
                    Logwin.Log("GameManager", 
                        $"Client retrieved {Players.Count} players from PlayerRegistry", "Multiplayer");
            }
            else
            {
                if (_enableDebugLog)
                    Logwin.LogError("GameManager", "PlayerRegistry.Instance is null on client!", "Multiplayer");
            }
        }

        [Rpc(SendTo.Server)]
        public virtual void PlayerResignedServerRpc(ulong _, string toString)
        {
            Debug.Log($"{toString} resigned");
            SetGameState(GameState.GameOver);
        }

        [Rpc(SendTo.Server)]
        public void ExitGameServerRpc()
        {
            CleanupSession();
        }

        protected virtual void CleanupSession()
        {
            GameSessionManager.Instance.CleanupSession();
        }

        void HandleGameStarted(GameStartedEvent e)
        {
            if (_enableDebugLog)
                Logwin.Log("GameManager", "GameStartedEvent received. Starting game...", "Multiplayer");
    
            // Get the player list from the PlayerRegistry
            if (PlayerRegistry.Instance)
            {
                Players = PlayerRegistry.Instance.GetPlayers();
                LocalPlayer = PlayerRegistry.Instance.LocalPlayer;
        
                if (_enableDebugLog)
                    Logwin.Log("GameManager", 
                        $"Retrieved {Players.Count} players from PlayerRegistry", "Multiplayer");
            }
            else
            {
                if (_enableDebugLog)
                    Logwin.LogError("GameManager", "PlayerRegistry.Instance is null!", "Multiplayer");
            }
    
            SetGameState(GameState.GameStarted);
        }
    }

    public enum GameState
    {
        None,
        WaitingForPlayers,
        GameStarted,
        GameRestarted,
        PlayerTurnStart,
        PlayerTurnEnd,
        GameOver
    }
}
