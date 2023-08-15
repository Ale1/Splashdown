using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable RedundantUsingDirective
using Options = Splashdown.Editor.Options;

namespace Splashdown.Editor
{
    public static class SplashdownUtils
    {
        public static void SaveOptionsToEditorPrefs(string key, Options opts)
        {
            if (opts == null)
                Debug.LogError("Splashdown :: saving null options in Editor Prefs");
            var value = JsonUtility.ToJson(opts);
            EditorPrefs.SetString(key, value);
        }
        

        public static Options GetOptionsFromGuid(string guid)
        {
            if (guid == null)
                return null;

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Options options = null;

            // Load all assets and sub-assets at the asset path.
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // Search for the "Options" sub-asset.
            foreach (var asset in allAssets)
            {
                if (asset is TextAsset textAsset && textAsset.name == Constants.GeneratedOptionsName)
                {
                    //Deserialize it.
                    options = JsonUtility.FromJson<Options>(textAsset.text);

                    if (options == null)
                        Debug.LogError("unable to deserialize");

                    break;
                }
            }

            if (options == null)
                Debug.LogError($"Unable to load Options from path: {assetPath}");

            return options;
        }

        public static string[] FindAllSplashdownGUIDs()
        {
            // Get all the .splashdown file guids in the AssetDatabase.
            string[] splashdownGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" }).Where(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return Path.GetExtension(path) == Constants.SplashdownExtension;
            }).ToArray();

            return splashdownGUIDs;
        }

        public static string GetGuidBySplashdownName(string name)
        {
            var splashdownGUIDs = FindAllSplashdownGUIDs();

            // Search for a .splashdown file with the target name.
            foreach (string guid in splashdownGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = Path.GetFileNameWithoutExtension(path);
                if (filename == name)
                {
                    // Found a .splashdown file with the target name.
                    return guid;
                }
            }

            // Didn't find a .splashdown file with the target name.
            return null;
        }

        public static Options GetOptionsFromSplashdownName(string targetName)
        {
            var guid = GetGuidBySplashdownName(targetName);
            if (guid == null)
            {
                Debug.LogError($"Splashdown :: {targetName} not found");
                return null;
            }

            return GetOptionsFromGuid(guid);
        }
        
        
        /// <summary>
        /// Fetches dynamic Splashdown options by searching all assemblies for a method marked with the Splashdown.OptionsProviderAttribute that has the correct signature.
        /// </summary>
        /// <remarks>
        /// If there are multiple methods in the assemblies that are marked with the Splashdown.OptionsProviderAttribute and have the correct signature,
        /// this method is non-deterministic and it's not guaranteed to return the options from the same method every time.
        /// </remarks>
        /// <returns>
        /// The Splashdown.Options returned by the first valid method it finds, or null if no valid method is found.
        /// </returns>
        public static Options FetchDynamicOptions(string targetName)
        {
            // Get all assemblies in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Static))
                    {
                        // method has the SplashdownOptionProviderAttribute
                        if (method.GetCustomAttributes(typeof(OptionsProviderAttribute), false)
                                .FirstOrDefault() is OptionsProviderAttribute)
                        {
                            // check for correct implementation by reading the return type
                            if (method.ReturnType != typeof(Options))
                            {
                                Debug.LogWarning(
                                    $"Splashdown :: {method} with {nameof(OptionsProviderAttribute)} does not have correct return type ");
                                continue;
                            }

                            // Check the parameters (should take none)
                            var parameters = method.GetParameters();
                            if (parameters.Length != 0)
                            {
                                Debug.LogWarning(
                                    $"Splashdown :: {method} with {nameof(OptionsProviderAttribute)} should not take any parameters ");
                                continue;
                            }
                            
                            //Check Name is a match
                            OptionsProviderAttribute attribute = (OptionsProviderAttribute)Attribute.GetCustomAttribute(method, typeof(OptionsProviderAttribute));
                            if(attribute.Filter != null && attribute.Filter != targetName)
                                continue;

                            // If we get here, the method is valid
                            var dynamicOptions = (Options)method.Invoke(null, null);
                            return dynamicOptions;
                        }
                    }
                }
            }

            return null; 
        }
    }
}
