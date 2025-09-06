using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MidniteOilSoftware.Core
{
    /// <summary>
    /// Non-generic class to handle runtime initialization for all SingletonMonoBehaviour instances.
    /// This is necessary because RuntimeInitializeOnLoadMethod doesn't work with static methods in generic classes.
    /// </summary>
    public static class SingletonInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitializeAllSingletons()
        {
            try
            {
                // Find all types that derive from SingletonMonoBehaviour<T> using reflection
                var singletonTypes = GetSingletonTypes();
                
                foreach (var type in singletonTypes)
                {
                    try
                    {
                        // Get the Instance property from each singleton type
                        var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        if (instanceProperty != null)
                        {
                            // Access the Instance property to trigger creation and initialization
                            var instance = instanceProperty.GetValue(null);
                            if (instance != null)
                            {
                                // Call OnRuntimeInitialize on the instance
                                var onRuntimeInitializeMethod = type.GetMethod("OnRuntimeInitialize", 
                                    BindingFlags.NonPublic | BindingFlags.Instance);
                                if (onRuntimeInitializeMethod != null)
                                {
                                    onRuntimeInitializeMethod.Invoke(instance, null);
                                    Debug.Log($"SingletonInitializer successfully initialized singleton: {type.Name}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SingletonInitializer] Failed to initialize singleton {type.Name}: {ex.Message}");
                    }
                }
                
                Debug.Log("All singleton initialization complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SingletonInitializer] Critical error during singleton initialization: {ex.Message}");
            }
        }

        static IEnumerable<Type> GetSingletonTypes()
        {
            var singletonTypes = new List<Type>();
            
            // Get all assemblies in the current domain
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Get all types from each assembly
                    var types = assembly.GetTypes();
                    
                    foreach (var type in types)
                    {
                        // Check if the type inherits from SingletonMonoBehaviour<T>
                        if (IsSingletonMonoBehaviour(type))
                        {
                            singletonTypes.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle cases where some types can't be loaded
                    Debug.LogWarning($"[SingletonInitializer] Could not load some types from assembly {assembly.FullName}: {ex.Message}");
                    
                    // Add the types that could be loaded
                    foreach (var type in ex.Types.Where(t => t != null && IsSingletonMonoBehaviour(t)))
                    {
                        singletonTypes.Add(type);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SingletonInitializer] Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }
            
            return singletonTypes;
        }

        static bool IsSingletonMonoBehaviour(Type type)
        {
            if (type == null || type.IsAbstract || type.IsInterface)
                return false;

            // Check if the type inherits from SingletonMonoBehaviour<T>
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && 
                    baseType.GetGenericTypeDefinition() == typeof(SingletonMonoBehaviour<>))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            
            return false;
        }
    }
}
