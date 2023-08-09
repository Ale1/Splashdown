using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
    }
}
