#if UNITY_EDITOR
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.AssetDatabase;

namespace MidniteOilSoftware.Core
{
    public static class ToolsMenu
    {
        [MenuItem("Midnite Oil Software/Core/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            CreateDirectories("_project", 
                "Animations", "Art", "Audio", "Audio/Sounds", "Audio/Music", 
                "Audio/Music/Music Clip Groups", "Audio/Music/Music Mixes", 
                "Fonts", "Materials", "Prefabs", "Scenes", "Scripts", "Textures");
            Refresh();
        }
        
        static void CreateDirectories(string root, params string[] directories)
        {
            var fullPath = Combine(dataPath, root);
            foreach (var newDirectory in directories)
            {
                CreateDirectory(Combine(fullPath, newDirectory));
            }
        }
        
        [MenuItem("CONTEXT/Canvas/Set Reference Resolution")]
        private static void SetReferenceResolution(MenuCommand command)
        {
            // Get the Canvas component that was right-clicked on
            var selectedCanvas = (Canvas)command.context;

            // Check if the Canvas has a CanvasScaler component
            var canvasScaler = selectedCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Debug.LogWarning($"CanvasScaler not found on {selectedCanvas.name}");
                return;
            }

            // Prompt the user for a resolution
            var input = EditorUtility.DisplayDialogComplex(
                    "Set Reference Resolution",
                    $"Set reference resolution for {selectedCanvas.name}.",
                    "1080p (1920x1080)",
                    "720p (1280x720)",
                    "Cancel"
                ) switch
                {
                    0 => "1920x1080",
                    1 => "1280x720",
                    _ => null
                };

            if (input == null)
            {
                Debug.Log("Operation canceled.");
                return;
            }

            // Parse the selected resolution
            var resolutionParts = input.Split('x');
            var selectedResolution = new Vector2(
                float.Parse(resolutionParts[0]),
                float.Parse(resolutionParts[1])
            );

            // Set the CanvasScaler properties
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = selectedResolution;

            Debug.Log($"Set reference resolution to {selectedResolution.x}x{selectedResolution.y} for {selectedCanvas.name}");
        }
    }
}
#endif