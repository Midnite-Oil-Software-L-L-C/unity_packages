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
        public GameState CurrentState => _currentState.Value;
        public List<GamePlayer> Players { get; private set; } = new();

        readonly NetworkVariable<GameState> _currentState = new();
        readonly int[] _playerOrder = new int[4];
        Unity.Services.Lobbies.Models.Lobby _game;
        int _currentPlayerTurnIndex = 0;

        void Start()
        {
            _currentState.OnValueChanged += GameState_OnValueChanged;
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (!IsHost) return;
                var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject
                    .GetComponent<GamePlayer>();
                JoinedGame(player);
            };
            _currentState.Value = GameState.WaitingForPlayers;
        }

        void GameState_OnValueChanged(GameState previousState, GameState newState)
        {
            Debug.Log(
                $"GameState changed from {previousState} to {newState} IsServer = {IsServer} IsClient = {IsClient} IsHost = {IsHost}");
            switch (newState)
            {
                case GameState.WaitingForPlayers:
                    AddAlreadyConnectedPlayers();
                    break;
                case GameState.GameStarted:
                    break;
                case GameState.PlayerTurnStart:
                    break;
                case GameState.PlayerTurnEnd:
                    break;
                case GameState.GamePaused:
                    break;
                case GameState.GameOver:
                    break;
            }

            EventBus.Instance.Raise(new GameStateChangedEvent(newState));
        }

        void JoinedGame(GamePlayer player)
        {
            if (Players.Contains(player))
            {
                Debug.Log($"{player.PlayerName} already added");
                return;
            }

            // if (player.IsLocalPlayer)
            // {
            //     _localPlayer = player;
            // }
            Players.Add(player);
            var expectedPlayers = LobbyManager.Instance.CurrentLobby.Players.Count;
            Debug.Log($"Adding {player} on server. {Players.Count}/{expectedPlayers} added");
            AddPlayerClientRpc(player.ConnectionId);
            if (Players.Count != expectedPlayers) return;
            RandomizePlayerOrder();
            if (!IsHost) return;
        }

        void AddAlreadyConnectedPlayers()
        {
            if (!IsHost) return;
            Debug.Log($"Adding already connected players ({NetworkManager.Singleton.ConnectedClientsList.Count})");
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                var networkPlayer = player.PlayerObject.GetComponent<GamePlayer>();
                JoinedGame(networkPlayer);
            }
        }

        void RandomizePlayerOrder()
        {
            var playerCount = Players.Count;
            for (var i = 0; i < playerCount; ++i)
            {
                _playerOrder[i] = i;
            }

            for (var i = 0; i < playerCount; ++i)
            {
                var temp = _playerOrder[i];
                var randomIndex = Random.Range(i, playerCount);
                _playerOrder[i] = _playerOrder[randomIndex];
                _playerOrder[randomIndex] = temp;
            }

            Debug.Log(
                $"Randomized player order is: {_playerOrder[0]}, {_playerOrder[1]}, {_playerOrder[2]}, {_playerOrder[3]}");
        }

        [Rpc(SendTo.NotServer, RequireOwnership = true)]
        void AddPlayerClientRpc(ulong connectionId)
        {
            NetworkManager.ConnectedClients.TryGetValue(connectionId, out var networkClient);
            var player = networkClient?.PlayerObject.GetComponent<GamePlayer>();
            if (player == null)
            {
                Debug.LogError($"Player not found for connectionId {connectionId}");
                return;
            }

            Players.Add(player);
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


