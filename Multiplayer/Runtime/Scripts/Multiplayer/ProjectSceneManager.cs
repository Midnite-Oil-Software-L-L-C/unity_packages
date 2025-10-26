using System;
using System.Collections;
using System.Collections.Generic;
using MidniteOilSoftware.Core;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidniteOilSoftware.Multiplayer.Lobby
{
    public class ProjectSceneManager : SingletonNetworkBehavior<ProjectSceneManager>
    {
        bool _unloading;
        string _scene;
        Scene _currentScene;

#if UNITY_EDITOR
        [SerializeField] SceneAsset _mainMenuScene;
        [SerializeField] SceneAsset _gameScene;

        void OnValidate()
        {
            if (_gameScene) _gameSceneName = _gameScene.name;
            if (_mainMenuScene) _mainMenuSceneName = _mainMenuScene?.name ?? "Main Menu";
        }
#endif
        [SerializeField] string _gameSceneName;
        [SerializeField] string _mainMenuSceneName = "Main Menu";

        public bool IsLoading { get; private set; }

        protected override void Start()
        {
            base.Start();
            LoadMainMenuScene();
        }

        [ContextMenu(nameof(SetupSceneManagementAndLoadGameScene))]
        public void SetupSceneManagementAndLoadGameScene()
        {
            if (!IsServer)
            {
                if (_enableDebugLog)
                {
                    Debug.LogError("Multiplayer:ProjectSceneManager - SceneManager Events registered on client, this should not be called.";
                }
                return;
            }

            if (_enableDebugLog)
            {
                Debug.Log("Multiplayer:ProjectSceneManager - SetupSceneManagementAndLoadNextTrack - Registering for Scene events on SERVER");
            }
            NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading = VerifySceneBeforeLoading;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompletedForAllPlayers;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleLoadCompleteForIndividualPlayer;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadEventCompletedForAllPlayers;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleLoadCompleteForIndividualPlayer;

            LoadGameScene();
        }
        
        void LoadMainMenuScene()
        {
            // If no main menu scene specified or already loaded, do nothing
            if (string.IsNullOrEmpty(_mainMenuSceneName) || 
                SceneManager.GetSceneByName(_mainMenuSceneName).IsValid()) return;

            if (_enableDebugLog) Debug.Log($"Multiplayer:ProjectSceneManager - Loading Main Menu Scene: {_mainMenuSceneName}");
            SceneManager.LoadSceneAsync(_mainMenuSceneName, LoadSceneMode.Single);
        }

        void LoadGameScene()
        {
            if (_enableDebugLog)
            {
                Debug.Log($"Multiplayer:ProjectSceneManager - Loading Game Scene: {_gameSceneName}");
            }
            StartCoroutine(LoadSceneAsync(_gameSceneName));
        }

        static bool VerifySceneBeforeLoading(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)
        {
            var isValid = sceneName != "Main Menu";
            Debug.Log($"Multiplayer:ProjectSceneManager - Doing Verification for {sceneName} (filtering out UserInterface, everything else passes verification. IsValid: {isValid})");
            return isValid;
        }

        IEnumerator LoadSceneAsync(string sceneName = default, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            IsLoading = true;
            if (_enableDebugLog) Logwin.Log("ProjectSceneManager", $"LoadSceneAsync {sceneName}", "Multiplayer");
            var currentScene = GetCurrentScene();

            if (currentScene != default && currentScene.Item1.name != sceneName)
            {
                if (_enableDebugLog) Debug.Log($"Multiplayer:ProjectSceneManager - Unloading current scene {currentScene.Item1.name}");
                yield return UnloadScene(currentScene.Item1);
            }
            
            while (NetworkManager.Singleton?.SceneManager == null) yield return null;

            try
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, loadSceneMode);
            }
            catch (Exception e)
            {
                Debug.LogError($"Multiplayer:ProjectSceneManager - Exception loading scene {sceneName} {e}");
                IsLoading = false;
            }
        }

        void HandleLoadCompleteForIndividualPlayer(
            ulong clientId, 
            string sceneName, 
            LoadSceneMode loadSceneMode)
        {
            if (!sceneName.Equals(_gameSceneName))
                return;

            if (_enableDebugLog)
            {
                Debug.Log($"Multiplayer:ProjectSceneManager - HandleLoadCompleteForIndividualPlayer: Scene {sceneName} loaded for client {clientId}");
            }

            // Try to get player object, but don't error if it's not available yet
            // Player objects might not be immediately available after scene load due to Netcode timing
            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (!playerNetworkObject)
            {
                if (_enableDebugLog)
                {
                    Debug.Log($"Multiplayer:ProjectSceneManager - PlayerNetworkObject not yet available for clientId {clientId} - this is normal during scene transitions");
                }
                
                // Optionally retry after a short delay
                StartCoroutine(RetryGetPlayerAfterDelay(clientId, sceneName, 0.5f));
                return;
            }

            ProcessPlayerForScene(clientId, playerNetworkObject);
        }

        IEnumerator RetryGetPlayerAfterDelay(ulong clientId, string sceneName, float delay)
        {
            yield return HelperFunctions.GetWaitForSeconds(delay);
            
            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerNetworkObject)
            {
                if (_enableDebugLog)
                {
                    Debug.Log($"Multiplayer:ProjectSceneManager - Successfully found PlayerNetworkObject for clientId {clientId} after delay");
                }
                ProcessPlayerForScene(clientId, playerNetworkObject);
            }
            else
            {
                if (_enableDebugLog)
                {
                    Debug.LogWarning($"Multiplayer:ProjectSceneManager - PlayerNetworkObject still not available for clientId {clientId} after {delay}s delay");
                }
            }
        }

        void ProcessPlayerForScene(ulong clientId, NetworkObject playerNetworkObject)
        {
            var player = playerNetworkObject.GetComponent<NetworkPlayer>();
            if (player == null)
            {
                if (_enableDebugLog)
                {
                    Debug.LogError($"Multiplayer:ProjectSceneManager - No NetworkPlayer component on PlayerNetworkObject for clientId {clientId}");
                }
                return;
            }

            // Now we can safely access player data
            var playerName = player.PlayerName.Value.Value;
            var playerId = player.PlayerId.Value.Value;
            
            if (_enableDebugLog)
            {
                Debug.Log($"Multiplayer:ProjectSceneManager - Player {playerName} (ID: {playerId}) loaded into scene {_gameSceneName}");
            }

            // Add any additional player processing logic here if needed
        }

        void HandleLoadEventCompletedForAllPlayers(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            if (_enableDebugLog)
            {
                Debug.Log($"Multiplayer:ProjectSceneManager - HandleLoadEventCompletedForAllPlayers {sceneName} {loadSceneMode} Completed:{clientsCompleted.Count} TimedOut:{clientsTimedOut.Count}");
            }
            IsLoading = false;
        }

        Tuple<Scene, string> GetCurrentScene()
        {
            for (var i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name != _gameSceneName) continue;
                _currentScene = scene;
                return new Tuple<Scene, string>(_currentScene, _currentScene.name);
            }

            return default;
        }

        IEnumerator UnloadScene(Scene scene)
        {
            // Assure only the server calls this when the NetworkObject is
            // spawned and the scene is loaded.
            if (!IsHost || !IsSpawned || !scene.IsValid() || !scene.isLoaded)
            {
                yield break;
            }

            _unloading = true;

            // Unload the scene
            NetworkManager.SceneManager.OnUnloadComplete -= UnloadComplete;
            NetworkManager.SceneManager.OnUnloadComplete += UnloadComplete;
            try
            {
                NetworkManager.SceneManager.UnloadScene(scene);
            }
            catch (Exception e)
            {
                NetworkManager.SceneManager.OnUnloadComplete -= UnloadComplete;
                Debug.LogError($"Multiplayer:ProjectSceneManager - Exception unloading scene {scene.name} {e}");
                _unloading = false;
            }
            while (_unloading) yield return null;
        }

        void UnloadComplete(ulong clientId, string sceneName)
        {
            NetworkManager.SceneManager.OnUnloadComplete -= UnloadComplete;
            _unloading = false;
        }

        public IEnumerator UnloadCurrentScene()
        {
            var currentScene = GetCurrentScene();
            if (_enableDebugLog)
            {
                Debug.Log($"Multiplayer:ProjectSceneManager - Unloading current scene {currentScene.Item1.name}");
            }
            yield return UnloadScene(currentScene.Item1);
        }
    }   
}
