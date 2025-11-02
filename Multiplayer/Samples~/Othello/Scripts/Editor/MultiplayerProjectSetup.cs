#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MidniteOilSoftware.Multiplayer.Samples.Othello.Editor
{
    public static class MultiplayerProjectSetup
    {
        [MenuItem("Midnite Oil Software/Multiplayer/Create Multiplayer Project Folders")]
        public static void CreateMultiplayerProjectFolders()
        {
            string projectName = EditorUtility.SaveFolderPanel(
                "Create Multiplayer Project Folders",
                Application.dataPath,
                "_project"
            );

            if (string.IsNullOrEmpty(projectName))
            {
                Debug.Log("Folder creation canceled.");
                return;
            }

            string rootFolderName = Path.GetFileName(projectName);
            
            if (string.IsNullOrEmpty(rootFolderName))
            {
                Debug.LogError("Invalid folder name.");
                return;
            }

            CreateDirectories(rootFolderName, 
                "Art",
                "Materials", 
                "Models", 
                "Music",
                "Prefabs", 
                "Scenes", 
                "Scripts", 
                "Scripts/Events",
                "Scripts/Editor",
                "Sounds",
                "Textures");
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Created multiplayer project folder structure under Assets/{rootFolderName}");
        }
        
        static void CreateDirectories(string root, params string[] directories)
        {
            string fullPath = Path.Combine(Application.dataPath, root);
            
            foreach (string newDirectory in directories)
            {
                string directoryPath = Path.Combine(fullPath, newDirectory);
                
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
        }
    }
}
#endif
