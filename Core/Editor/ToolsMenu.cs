using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;
using UnityEditor;
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
    }
}
