using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Reflection;

namespace MidniteOilSoftware.Multiplayer.Editor
{
    [InitializeOnLoad]
    public static class TMPAutoImporter
    {
        private const string TMPSettingsAssetPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string TMPExamplesFontPath = "Assets/TextMesh Pro/Fonts/Bangers-Regular SDF.asset";

        static TMPAutoImporter()
        {
            UnityEditor.PackageManager.Events.registeredPackages += OnRegisteredPackages;
            EditorApplication.delayCall += CheckAndImportTMPResources;
        }

        private static void OnRegisteredPackages(PackageRegistrationEventArgs args)
        {
            foreach (var packageInfo in args.added)
            {
                if (packageInfo.name is not ("com.unity.textmeshpro" or "com.midniteoilsoftware.multiplayer")) continue;
                EditorApplication.delayCall += CheckAndImportTMPResources;
                return;
            }
        }

        private static void CheckAndImportTMPResources()
        {
            Assembly tmproAssembly = null;
            try
            {
                tmproAssembly = System.Reflection.Assembly.Load("Unity.TextMeshPro.Editor");
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }

            if (tmproAssembly == null) return;

            bool essentialsImported = AssetDatabase.LoadAssetAtPath<Object>(TMPSettingsAssetPath) != null;
            bool examplesImported = AssetDatabase.LoadAssetAtPath<Object>(TMPExamplesFontPath) != null;

            if (!essentialsImported)
            {
                Debug.Log("[Midnite Oil Multiplayer] Importing TextMesh Pro Essential Resources...");
                ImportTMPEssentials(tmproAssembly);
            }

            if (!examplesImported)
            {
                Debug.Log("[Midnite Oil Multiplayer] Importing TextMesh Pro Examples & Extras (including Bangers font)...");
                ImportTMPExamples(tmproAssembly);
            }

            if (essentialsImported && examplesImported)
            {
                Debug.Log("[Midnite Oil Multiplayer] TextMesh Pro resources already imported.");
            }
        }

        private static void ImportTMPEssentials(Assembly tmproAssembly)
        {
            try
            {
                var tmproPackageUtilitiesType = tmproAssembly.GetType("TMPro.TMP_PackageUtilities");
                if (tmproPackageUtilitiesType != null)
                {
                    var importMethod = tmproPackageUtilitiesType.GetMethod("ImportProjectResourcesMenu", 
                        BindingFlags.Static | BindingFlags.Public);
                    if (importMethod != null)
                    {
                        importMethod.Invoke(null, null);
                        Debug.Log("[Midnite Oil Multiplayer] TMP Essential Resources imported successfully!");
                        return;
                    }
                }
                Debug.LogWarning("[Midnite Oil Multiplayer] Could not auto-import TMP Essentials. " +
                    "Please import manually via: Window > TextMeshPro > Import TMP Essential Resources");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Midnite Oil Multiplayer] Error auto-importing TMP Essentials: {e.Message}. " +
                    "Please import manually via: Window > TextMeshPro > Import TMP Essential Resources");
            }
        }

        private static void ImportTMPExamples(Assembly tmproAssembly)
        {
            try
            {
                var tmproPackageUtilitiesType = tmproAssembly.GetType("TMPro.TMP_PackageUtilities");
                if (tmproPackageUtilitiesType != null)
                {
                    var importMethod = tmproPackageUtilitiesType.GetMethod("ImportExamplesContentMenu", 
                        BindingFlags.Static | BindingFlags.Public);
                    if (importMethod != null)
                    {
                        importMethod.Invoke(null, null);
                        Debug.Log("[Midnite Oil Multiplayer] TMP Examples & Extras imported successfully!");
                        return;
                    }
                }
                Debug.LogWarning("[Midnite Oil Multiplayer] Could not auto-import TMP Examples & Extras. " +
                    "Please import manually via: Window > TextMeshPro > Import TMP Examples and Extras");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Midnite Oil Multiplayer] Error auto-importing TMP Examples & Extras: {e.Message}. " +
                    "Please import manually via: Window > TextMeshPro > Import TMP Examples and Extras");
            }
        }
    }
}
