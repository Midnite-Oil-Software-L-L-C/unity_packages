using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


#if UNITY_EDITOR
        [SerializeField] List<SceneAsset> _scenes;

        void OnValidate()
        {
            _sceneNames = _scenes.Select(s => s.name).ToList();
        }
#endif
        [SerializeField] List<string> _sceneNames;

        public bool IsLoading { get; private set; }

        public Scene CurrentScene => GetCurrentScene().Item1;

        void Start()
        {
            NetworkManager.Singleton.OnServerStarted += SetupSceneManagementAndLoadGameScene;
        }

        void ListScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Debug.Log(SceneManager.GetSceneAt(i).name);
            }
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

            StartCoroutine(LoadSceneAsync("Othello"));
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

        public void LoadScene(string trackName) => StartCoroutine(LoadSceneAsync(trackName));

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
            if (sceneName.Equals("Reversi") == false)
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
            // NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompletedForAllPlayers;
            ListScenes();
        }

        Tuple<Scene, string> GetCurrentScene()
        {
            for (var i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                var currentScene = SceneManager.GetSceneAt(i);
                _scene = _sceneNames.FirstOrDefault(t => t == currentScene.name);
                if (_scene != null)
                    return new Tuple<Scene, string>(currentScene, _scene);
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

        void UnloadComplete(ulong clientid, string scenename)
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