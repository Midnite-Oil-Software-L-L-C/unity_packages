using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace MidniteOilSoftware.Multiplayer.Editor
{
    public static class TMPManualImporter
    {
        [MenuItem("Midnite Oil Software/Multiplayer/Import TMP Essential Resources")]
        public static void ImportTMPEssentials()
        {
            var tmproAssembly = GetTMPAssembly();
            if (tmproAssembly == null)
            {
                Debug.LogError("[Midnite Oil Multiplayer] TextMesh Pro assembly not found. Please ensure TextMesh Pro is installed.");
                return;
            }

            Debug.Log("[Midnite Oil Multiplayer] Importing TextMesh Pro Essential Resources...");
            InvokeTMPImport(tmproAssembly, "ImportProjectResourcesMenu", "TMP Essential Resources");
        }

        [MenuItem("Midnite Oil Software/Multiplayer/Import TMP Examples & Extras")]
        public static void ImportTMPExamples()
        {
            var tmproAssembly = GetTMPAssembly();
            if (tmproAssembly == null)
            {
                Debug.LogError("[Midnite Oil Multiplayer] TextMesh Pro assembly not found. Please ensure TextMesh Pro is installed.");
                return;
            }

            Debug.Log("[Midnite Oil Multiplayer] Importing TextMesh Pro Examples & Extras...");
            InvokeTMPImport(tmproAssembly, "ImportExamplesContentMenu", "TMP Examples & Extras");
        }

        static Assembly GetTMPAssembly()
        {
            try
            {
                return Assembly.Load("Unity.TextMeshPro.Editor");
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        static void InvokeTMPImport(Assembly tmproAssembly, string methodName, string resourceName)
        {
            try
            {
                var tmproPackageUtilitiesType = tmproAssembly.GetType("TMPro.TMP_PackageUtilities");
                if (tmproPackageUtilitiesType != null)
                {
                    var importMethod = tmproPackageUtilitiesType.GetMethod(methodName, 
                        BindingFlags.Static | BindingFlags.Public);
                    if (importMethod != null)
                    {
                        importMethod.Invoke(null, null);
                        Debug.Log($"[Midnite Oil Multiplayer] {resourceName} imported successfully!");
                        return;
                    }
                }
                Debug.LogWarning($"[Midnite Oil Multiplayer] Could not import {resourceName}. Method not found.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Midnite Oil Multiplayer] Error importing {resourceName}: {e.Message}");
            }
        }
    }
}
