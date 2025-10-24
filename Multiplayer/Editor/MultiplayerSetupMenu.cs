#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Build.Profile;

namespace MidniteOilSoftware.Multiplayer.Editor
{
    public static class MultiplayerSetupMenu
    {
        const string PACKAGE_SCENES_PATH = "Packages/com.midniteoilsoftware.multiplayer/Runtime/Scenes";
        const string TARGET_SCENES_PATH = "Assets/Scenes";
        
        [MenuItem("Midnite Oil Software/Multiplayer/Setup Multiplayer Scenes")]
        public static void SetupMultiplayerScenes()
        {
            if (!EditorUtility.DisplayDialog(
                "Setup Multiplayer Scenes",
                "This will:\n\n" +
                "1. Copy Bootstrapper.unity and Main Menu.unity to Assets/Scenes/\n" +
                "2. Add them to your active Build Profile\n\n" +
                "Continue?",
                "Yes",
                "Cancel"))
            {
                return;
            }

            try
            {
                EnsureScenesDirectoryExists();
                CopySceneFromPackage("Bootstrapper.unity");
                CopySceneFromPackage("Main Menu.unity");
                AddScenesToBuildProfile();
                
                EditorUtility.DisplayDialog(
                    "Success",
                    "Multiplayer scenes have been copied to Assets/Scenes/ and added to your Build Profile.\n\n" +
                    "Build order:\n" +
                    "0. Bootstrapper\n" +
                    "1. Main Menu\n" +
                    "(Plus any existing scenes)",
                    "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Failed to setup multiplayer scenes:\n\n{e.Message}",
                    "OK");
                Debug.LogError($"Failed to setup multiplayer scenes: {e}");
            }
        }
        
        static void EnsureScenesDirectoryExists()
        {
            if (!Directory.Exists(TARGET_SCENES_PATH))
            {
                Directory.CreateDirectory(TARGET_SCENES_PATH);
                AssetDatabase.Refresh();
                Debug.Log($"Created directory: {TARGET_SCENES_PATH}");
            }
        }
        
        static void CopySceneFromPackage(string sceneName)
        {
            string sourcePathRelative = $"{PACKAGE_SCENES_PATH}/{sceneName}";
            string targetPathRelative = $"{TARGET_SCENES_PATH}/{sceneName}";
            
            if (File.Exists(targetPathRelative))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Scene Already Exists",
                    $"{sceneName} already exists in {TARGET_SCENES_PATH}.\n\nOverwrite it?",
                    "Yes",
                    "No");
                
                if (!overwrite)
                {
                    Debug.Log($"Skipped copying {sceneName} (already exists)");
                    return;
                }
            }
            
            bool success = AssetDatabase.CopyAsset(sourcePathRelative, targetPathRelative);
            
            if (success)
            {
                Debug.Log($"Copied {sceneName} to {TARGET_SCENES_PATH}");
            }
            else
            {
                throw new System.Exception($"Failed to copy {sceneName}. Make sure the package is installed correctly.");
            }
        }
        
        static void AddScenesToBuildProfile()
        {
            string bootstrapperPath = $"{TARGET_SCENES_PATH}/Bootstrapper.unity";
            string mainMenuPath = $"{TARGET_SCENES_PATH}/Main Menu.unity";
            
            var existingScenes = EditorBuildSettings.scenes;
            var scenesList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            
            scenesList.Add(new EditorBuildSettingsScene(bootstrapperPath, true));
            scenesList.Add(new EditorBuildSettingsScene(mainMenuPath, true));
            
            foreach (var scene in existingScenes)
            {
                if (scene.path != bootstrapperPath && scene.path != mainMenuPath)
                {
                    scenesList.Add(scene);
                }
            }
            
            EditorBuildSettings.scenes = scenesList.ToArray();
            
            Debug.Log($"Added scenes to Build Profile. Total scenes: {scenesList.Count}");
            Debug.Log("Scene build order:");
            for (int i = 0; i < scenesList.Count; i++)
            {
                Debug.Log($"  {i}. {scenesList[i].path}");
            }
        }
    }
}
#endif
