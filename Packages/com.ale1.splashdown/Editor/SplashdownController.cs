
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.iOS;
using UnityEngine;

namespace Splashdown.Editor
{
    public static class SplashdownController
    {

        public static string[] FindAllSplashdownFiles()
        {
            // Get all the .splashdown file guids in the AssetDatabase.
            string[] splashdownGUIDs = AssetDatabase.FindAssets("", new[] { "Assets" }).Where(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return Path.GetExtension(path) == ".splashdown";
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
        
        public static Options LoadOptionsFromSplashdownFile(string guid)
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
                if (asset is TextAsset textAsset && textAsset.name == "Options")
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
                Debug.LogError($"{targetName} not found");
                return;
            }
            
            var splashdownData = LoadOptionsFromSplashdownFile(guid);
            if (splashdownData != null)
            {
                var handler = new LogoHandler(splashdownData);
                handler.SetSplash();
            }
        }
        
        public static void RemoveSplash(string targetName)
        {
            var guid = FindSplashdownByName(targetName);
            var splashDownData = LoadOptionsFromSplashdownFile(guid);
            var handler = new LogoHandler(splashDownData);
            handler.RemoveSplash();
        }

        private static Dictionary<PlatformIconKind, PlatformIcon[]> _backupIcons;


        public static bool SetIcons(string targetName)
        {
            var guid = FindSplashdownByName(targetName);
            if (guid == null)
            {
                Debug.LogError($"{targetName} not found");
                return false;
            }
            
            var splashdownData = LoadOptionsFromSplashdownFile(guid);
            if (splashdownData == null)
            {
                Debug.LogError("could not load options");
                return false;
            }
              

            var sprite = splashdownData.Sprite;
            
            var transparentBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.ale1.splashdown/Editor/splashdown_transparent.png");  //todo: move to constants

            if (sprite == null || transparentBackground == null)
            {
                Debug.LogError("error loading sprites");
                return false;
            }

            NamedBuildTarget platform;
            PlatformIconKind[] kinds;
            if (!GetPlatform(out platform) || !FetchIconKind(out kinds))
            {
                return false;
            }

            //fill backup icons dictionary.  
            _backupIcons = new Dictionary<PlatformIconKind, PlatformIcon[]>();
            foreach (var kind in kinds)
            {
                var icons = PlayerSettings.GetPlatformIcons(platform, kind);
                _backupIcons[kind] = icons;
            }

            foreach (var kind in kinds)
            {
                var icons = PlayerSettings.GetPlatformIcons(platform, kind);
                //Assign textures to each available icon slot.
                for (int i = 0; i < icons.Length; i++)
                {
                    var size = icons[i].maxLayerCount; // amount of textures expected
                    var splashdownIconArray = new Texture2D[size];
                    int index = size > 1 ? 1 : 0;
                    splashdownIconArray[0] = transparentBackground; // background transparent by default
                    splashdownIconArray[index] = sprite.texture; //set foreground to splashdown
                    icons[i].SetTextures(splashdownIconArray);
                }

                PlayerSettings.SetPlatformIcons(platform, kind, icons);
            }
            return true;
        }
        
        public static bool RestoreIcons()
        {
            if (_backupIcons == null)
                return false;
            
            if (GetPlatform(out NamedBuildTarget platform))
            {
                foreach (var entry in _backupIcons)
                {
                    PlayerSettings.SetPlatformIcons(platform, entry.Key, entry.Value);
                }

                return true;
            }

            return false;
        }

        private static bool FetchIconKind(out PlatformIconKind[] kinds)
        {
            BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (currentBuildTarget)
            {
                case BuildTarget.Android:
                    kinds = new[]
                    {
                        AndroidPlatformIconKind.Adaptive, AndroidPlatformIconKind.Legacy,
                        AndroidPlatformIconKind.Round
                    };
                    return true;
                case BuildTarget.iOS:
                    kinds = new[] { iOSPlatformIconKind.Application };
                    return true;
                default:
                    Debug.LogError("unsupported platform");
                    kinds = null;
                    return false;
            }
        }

        private static bool GetPlatform(out NamedBuildTarget platform)
        {
            BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (currentBuildTarget)
            {
                case BuildTarget.Android:
                    platform = NamedBuildTarget.Android;
                    return true;
                case BuildTarget.iOS:
                    platform = NamedBuildTarget.iOS;
                    return true;
                default:
                    platform = NamedBuildTarget.Unknown;
                    Debug.LogWarning("unsupported build target");
                    return false;
            }
        }
    }
}