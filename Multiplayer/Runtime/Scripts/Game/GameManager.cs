using System.Collections.Generic;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.Multiplayer.Events;
using MidniteOilSoftware.Multiplayer.Lobby;
using Unity.Netcode;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer
{
    public class GameManager : NetworkBehaviour
    {
        public GameState CurrentState { get; protected set; } = GameState.None;
        public int CurrentPlayerTurnIndex => _currentPlayerTurnIndex.Value;
        public NetworkPlayer CurrentPlayer
        {
            get
            {
                if (CurrentPlayerTurnIndex >= Players.Count)
                {
                    Debug.LogError($"CurrentPlayerTurnIndex {CurrentPlayerTurnIndex} out of range of {Players.Count} players");
                    return null;
                }
                return Players[CurrentPlayerTurnIndex];
            }
        }

        public NetworkPlayer LocalPlayer { get; protected set; }

        protected List<NetworkPlayer> Players { get; private set; } = new();
        protected NetworkVariable<int> _currentPlayerTurnIndex = new();
        Unity.Services.Lobbies.Models.Lobby Game => LobbyManager.Instance.CurrentLobby;

        protected virtual void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (!IsHost) return;
                var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject
                    .GetComponent<NetworkPlayer>();
                JoinedGame(player);
            };
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
            Debug.Log(
                $"GameState changed from {previousState} to {newState} IsServer = {IsServer} IsClient = {IsClient} IsHost = {IsHost}");

            if (IsServer)
            {
                ServerOnlyHandleGameStateChange();
            }
            else
            {
                ClientOnlyHandleGameStateChange();
            }

            Debug.Log($"Raising GameStateChangedEvent({newState})", this);
            EventBus.Instance.Raise(new GameStateChangedEvent(newState));
        }

        protected virtual void ServerOnlyHandleGameStateChange()
        {
            switch (CurrentState)
            {
                case GameState.WaitingForPlayers:
                    AddAlreadyConnectedPlayers();
                    break;
                case GameState.GameStarted:
                    _currentPlayerTurnIndex.Value = 0;
                    SetGameState(GameState.PlayerTurnStart, 0.25f);
                    break;
                case GameState.PlayerTurnEnd:
                    if (GameOver())
                    {
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
                    GetConnectedPlayers();
                    break;
            }
        }

        protected virtual bool GameOver()
        {
            throw new System.NotImplementedException("You must override this method in a subclass");
        }

        protected virtual void JoinedGame(NetworkPlayer player)
        {
            if (!IsHost) return;

            if (Players.Contains(player))
            {
                Debug.Log($"{player.PlayerName} already added. IsServer = {IsServer} IsClient = {IsClient} IsHost = {IsHost}");
                return;
            }

            if (player.IsLocalPlayer)
            {
                LocalPlayer = player;
            }
            Players.Add(player);
            var expectedPlayers = Game.Players.Count;
            Debug.Log($"Adding {player} on server. {Players.Count}/{expectedPlayers} added");
            if (Players.Count != expectedPlayers) return;
            SetGameState(GameState.GameStarted);
        }
        
        void GetConnectedPlayers()
        {
            if (IsHost) return;
            Debug.Log($"Getting connected players ({NetworkManager.Singleton.ConnectedClientsList.Count})");
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                var networkPlayer = player.PlayerObject.GetComponent<NetworkPlayer>();
                if (networkPlayer.IsLocalPlayer)
                {
                    LocalPlayer = networkPlayer;
                }
                Players.Add(networkPlayer);
            }
        }

        void AddAlreadyConnectedPlayers()
        {
            if (!IsHost) return;
            Debug.Log($"Adding already connected players ({NetworkManager.Singleton.ConnectedClientsList.Count})");
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                var networkPlayer = player.PlayerObject.GetComponent<NetworkPlayer>();
                JoinedGame(networkPlayer);
            }
        }
    }

    public enum GameState
    {
        None,
        WaitingForPlayers,
        GameStarted,
        PlayerTurnStart,
        PlayerTurnEnd,
        GamePaused,
        GameOver
    }
}


