
using System.Collections.Generic;
using System.IO;
using Splashdown.Editor;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.iOS;
using UnityEngine;

namespace Splashdown.Editor
{
    public static class SplashdownController
    {
        public static bool validated = false;
        
        private static Dictionary<string, Options> SplashdownRegistry;
        private static Options LoadFromRegistry(string guid) => SplashdownRegistry[guid];

        private static Options LoadFromAssetDatabase(string guid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            Splashdown.Options options = null;

            // Load all assets and sub-assets at the asset path.
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            // Search for the "Options" sub-asset.
            foreach (var asset in allAssets)
            {
                if (asset is TextAsset textAsset && textAsset.name == "Options")
                {
                    // Found the "Options" sub-asset. Deserialize it.
                    options = JsonUtility.FromJson<Splashdown.Options>(textAsset.text);
                    
                    if(options == null)
                        Debug.LogError("unable to deserialize");
                    
                    break;
                }
            }

            if(options == null)
                Debug.LogError($"Unable to load Options from path: {assetPath}");
    
            return options;
        }
        
        public static void ValidateRegistry()
        {
            if (SplashdownRegistry == null)
            {
                SplashdownRegistry = new Dictionary<string, Options>();
            }

            string[] assetPaths = AssetDatabase.GetAllAssetPaths();

            // Create a HashSet containing all the GUIDs of .splashdown files in the project.
            HashSet<string> splashdownGuids = new HashSet<string>();
            foreach (var assetPath in assetPaths)
            {
                string extension = Path.GetExtension(assetPath);
                // Ignore directories that end in .splashdown, like the package folder!
                if (extension == ".splashdown" && !Directory.Exists(assetPath))
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    splashdownGuids.Add(guid);
            
                    if (!SplashdownRegistry.ContainsKey(guid))
                    { 
                        SplashdownRegistry[guid] = LoadFromAssetDatabase(guid);
                    }
                }
            }

            // Remove any keys from the registry that are not in the set of splashdown GUIDs.
            List<string> keysToRemove = new List<string>();
            foreach (var guid in SplashdownRegistry.Keys)
            {
                if (!splashdownGuids.Contains(guid))
                {
                    keysToRemove.Add(guid);
                }
            }

            foreach (var guid in keysToRemove)
            {
                SplashdownRegistry.Remove(guid);
            }

            validated = true;
        }
        
        private static Options FindByName(string targetName)
        {
            if(!validated)
                ValidateRegistry();
            
            foreach (var kvp in SplashdownRegistry)
            {
                var options = kvp.Value;

                if(options == null)
                {
                    Debug.LogError("there is an empty options in the splashdown registry");
                }
                
                if (options.fileName == targetName)
                {
                    return options;
                }
            }
            return null;
        }

        public static void SetSplash(string targetName)
        {
            if(!validated)
                ValidateRegistry();
            
            var splashdownData = FindByName(targetName);
            if (splashdownData != null)
            {
                var handler = new LogoHandler(splashdownData);
                handler.SetSplash();
            }
            else
            {
                Debug.LogError($"{targetName} not found");
            };
        }

        public static void RemoveSplash(string targetName)
        {
            var splashdown = FindByName(targetName);
            var handler = new LogoHandler(splashdown);
            handler.RemoveSplash();
            
        }

        
        private static Dictionary<PlatformIconKind, PlatformIcon[]> _backupIcons;

        private static bool SetIcons(Sprite sprite)
        {
            var transparentBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.ale1.splashdown/Editor/splashdown_transparent.png");  //todo: move to constants

            if (sprite == null || transparentBackground == null)
            {
                Debug.LogError("error loading sprites");
                return false;
            }

            NamedBuildTarget platform;
            PlatformIconKind[] kinds;
            if (!FetchPlatform(out platform) || !FetchIconKind(out kinds))
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
        
        private static void RestoreIcons()
        {
            if (FetchPlatform(out NamedBuildTarget platform))
            {
                foreach (var entry in _backupIcons)
                {
                    PlayerSettings.SetPlatformIcons(platform, entry.Key, entry.Value);
                }
            }
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

        private static bool FetchPlatform(out NamedBuildTarget platform)
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