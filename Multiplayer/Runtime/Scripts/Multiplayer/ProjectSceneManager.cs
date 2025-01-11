using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [SerializeField] SceneAsset _menuScene, _gameScene;

        void OnValidate()
        {
            _menuSceneName = _menuScene.name;
            _gameSceneName = _gameScene.name;
        }
#endif
        [SerializeField] string _menuSceneName, _gameSceneName;

        public bool IsLoading { get; private set; }

        void Start()
        {
            NetworkManager.Singleton.OnServerStarted += SetupSceneManagementAndLoadGameScene;
        }

        [ContextMenu(nameof(SetupSceneManagementAndLoadGameScene))]
        public void SetupSceneManagementAndLoadGameScene()
        {
            if (!IsServer)
            {
                Debug.LogError("SceneManager Events registered on client, this should not be called.");
                return;
            }

            Debug.Log("SetupSceneManagementAndLoadNextTrack - Registering for Scene events on SERVER");
            NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading = VerifySceneBeforeLoading;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadEventCompletedForAllPlayers;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleLoadCompleteForIndividualPlayer;

            LoadGameScene();
        }

        public void LoadGameScene()
        {
            Debug.Log("Loading Game Scene");

            StartCoroutine(LoadSceneAsync(_gameSceneName));
        }

        void LogSceneEvent(SceneEvent sceneEvent)
        {
            Debug.Log("SceneEvent " + sceneEvent.SceneEventType + " " + sceneEvent.SceneName + " for player " +
                      sceneEvent.ClientId);
            Debug.Log(sceneEvent.ClientsThatCompleted?.Count + " clients completed" +
                      sceneEvent.ClientsThatTimedOut?.Count + " clients timed out");
            if (sceneEvent.ClientsThatTimedOut != null)
                foreach (var client in sceneEvent.ClientsThatTimedOut)
                {
                    Debug.Log(client + " timed out");
                }
        }

        static bool VerifySceneBeforeLoading(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log("Doing Verification for " + sceneName +
                      " (filtering out UserInterface, everything else passes verification");

            return sceneName != "Main Menu";
        }

        public void LoadScene(string sceneName) => StartCoroutine(LoadSceneAsync(sceneName));

        IEnumerator LoadSceneAsync(string sceneName = default)
        {
            IsLoading = true;
            var currentScene = GetCurrentScene();

            if (currentScene != default)
                yield return UnloadScene(currentScene.Item1);

            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            Debug.Log($"Loading Scene {sceneName}");
        }

        void HandleLoadCompleteForIndividualPlayer(
            ulong clientId, 
            string sceneName, 
            LoadSceneMode loadSceneMode)
        {
            if (sceneName.Equals(_gameSceneName) == false)
                return;

            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            var player = playerNetworkObject.GetComponent<NetworkPlayer>();
            var playerName = player.PlayerName.Value.Value;
            var playerId = player.PlayerId.Value.Value;
        }

        void HandleLoadEventCompletedForAllPlayers(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            Debug.Log(
                $"HandleLoadEventCompletedForAllPlayers {sceneName} {loadSceneMode} Completed:{clientsCompleted.Count} TimedOut:{clientsTimedOut.Count}");
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
            NetworkManager.SceneManager.OnUnloadComplete += UnloadComplete;
            NetworkManager.SceneManager.UnloadScene(scene);
            while (_unloading)
                yield return null;
        }

        void UnloadComplete(ulong clientId, string sceneName)
        {
            NetworkManager.SceneManager.OnUnloadComplete -= UnloadComplete;
            _unloading = false;
        }

        public IEnumerator LoadMenu()
        {
            var currentScene = GetCurrentScene();
            yield return UnloadScene(currentScene.Item1);
        }

        public async Task UnloadCurrentScene()
        {
            var currentTrack = GetCurrentScene();
            await SceneManager.UnloadSceneAsync(currentTrack.Item1);
        }
    }
}