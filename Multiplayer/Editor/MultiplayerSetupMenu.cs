#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Editor
{
    public static class MultiplayerSetupMenu
    {
        const string BOOTSTRAPPER_SCENE_PATH = "Packages/com.midniteoilsoftware.multiplayer/Runtime/Scenes/Bootstrapper.unity";
        const string MAIN_MENU_SCENE_PATH = "Packages/com.midniteoilsoftware.multiplayer/Runtime/Scenes/Main Menu.unity";
        
        [MenuItem("Midnite Oil Software/Multiplayer/Add Package Scenes to Build Settings")]
        public static void AddPackageScenesToBuildSettings()
        {
            if (!EditorUtility.DisplayDialog(
                "Add Package Scenes to Build Settings",
                "This will add the Bootstrapper and Main Menu scenes from the Multiplayer package directly to your Build Settings.\n\n" +
                "The scenes will remain in the package (read-only) but will be included in your builds.\n\n" +
                "Continue?",
                "Yes",
                "Cancel"))
            {
                return;
            }

            try
            {
                var existingScenes = EditorBuildSettings.scenes;
                var scenesList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
                
                bool bootstrapperExists = false;
                bool mainMenuExists = false;
                
                foreach (var scene in existingScenes)
                {
                    if (scene.path == BOOTSTRAPPER_SCENE_PATH)
                        bootstrapperExists = true;
                    if (scene.path == MAIN_MENU_SCENE_PATH)
                        mainMenuExists = true;
                }
                
                if (!bootstrapperExists)
                {
                    scenesList.Add(new EditorBuildSettingsScene(BOOTSTRAPPER_SCENE_PATH, true));
                    Debug.Log($"Added Bootstrapper scene to Build Settings");
                }
                
                if (!mainMenuExists)
                {
                    scenesList.Add(new EditorBuildSettingsScene(MAIN_MENU_SCENE_PATH, true));
                    Debug.Log($"Added Main Menu scene to Build Settings");
                }
                
                foreach (var scene in existingScenes)
                {
                    scenesList.Add(scene);
                }
                
                EditorBuildSettings.scenes = scenesList.ToArray();
                
                string message = "Package scenes have been added to Build Settings!\n\n";
                message += "Build order:\n";
                for (int i = 0; i < scenesList.Count; i++)
                {
                    message += $"{i}. {System.IO.Path.GetFileNameWithoutExtension(scenesList[i].path)}\n";
                }
                
                EditorUtility.DisplayDialog("Success", message, "OK");
                
                Debug.Log($"Build Settings updated. Total scenes: {scenesList.Count}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Failed to add package scenes:\n\n{e.Message}",
                    "OK");
                Debug.LogError($"Failed to add package scenes: {e}");
            }
        }
    }
}
#endif
