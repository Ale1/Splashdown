
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Constants = Splashdown.Constants;

namespace Splashdown.Editor
{
    public static class SplashdownController
    {
        public static IconHandler iconHandler;

        public static string[] FindAllSplashdownFiles()
        {
            // Get all the .splashdown file guids in the AssetDatabase.
            string[] splashdownGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" }).Where(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return Path.GetExtension(path) == Constants.SplashdownExtension;
            }).ToArray();

            return splashdownGUIDs;
        }
        
        public static string FindSplashdownByName(string name)
        {
            var splashdownGUIDs = FindAllSplashdownFiles();
            
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
        
        public static Options GetOptionsFromFileName(string targetName)
        {
            var guid = FindSplashdownByName(targetName);
            if (guid == null)
            {
                Debug.LogError($"Splashdown :: {targetName} not found");
                return null;
            }

            return GetOptionsFromGUID(guid);
        }
        
        public static Options GetOptionsFromGUID(string guid)
        {
            if (guid == null)
                return null;
            
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Splashdown.Editor.Options options = null;

            // Load all assets and sub-assets at the asset path.
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // Search for the "Options" sub-asset.
            foreach (var asset in allAssets)
            {
                if (asset is TextAsset textAsset && textAsset.name == Constants.GeneratedOptionsName)
                {
                    // Found the "Options" sub-asset. Deserialize it.
                    options = JsonUtility.FromJson<Splashdown.Editor.Options>(textAsset.text);
                    
                    if(options == null)
                        Debug.LogError("unable to deserialize");
                    
                    break;
                }
            }

            if(options == null)
                Debug.LogError($"Unable to load Options from path: {assetPath}");
    
            return options;
        }

        public static void SetSplash(string targetName)
        {
            var guid = FindSplashdownByName(targetName);
            if (guid == null)
            {
                Debug.LogError($"Splashdown :: {targetName} not found");
                return;
            }
            
            var splashdownData = GetOptionsFromGUID(guid);
            if (splashdownData != null)
            {
                var handler = new LogoHandler(splashdownData);
                handler.SetSplash();
            }
        }
        
        public static void RemoveSplash(string targetName)
        {
            var splashdownData = GetOptionsFromFileName(targetName);
            
            if (splashdownData == null)
                return;
            var handler = new LogoHandler(splashdownData);
            handler.RemoveSplash();
        }

        
        public static void SetIcons(string targetName)
        {
            if (iconHandler != null)
            {
                iconHandler.RestoreIcons();
                iconHandler.Dispose();
                iconHandler = null;
            }

            var guid = FindSplashdownByName(targetName);
            if (guid == null)
            {
                Debug.LogError($"{targetName} not found");
                return;
            }
            
            var splashdownData = GetOptionsFromGUID(guid);
            if (splashdownData == null)
            {
                Debug.LogError("could not load options");
                return;
            }
            
            iconHandler = new IconHandler(splashdownData);
            iconHandler.SetIcons();
        }
        
        public static void RestoreIcons()
        {
            if (iconHandler != null)
            {
                Debug.Log("restoring");
                iconHandler.RestoreIcons();
                iconHandler.Dispose();
                iconHandler = null;
            }
        }
        
        
    }
}