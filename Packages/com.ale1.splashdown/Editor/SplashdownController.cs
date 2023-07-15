using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.Android;

namespace Splashdown.Editor
{
    public static class SplashdownController
    {
        private static Dictionary<string, SplashdownImporter> SplashdownRegistry;
        private static SplashdownImporter LoadFromRegistry(string guid) => SplashdownRegistry[guid];

        private static SplashdownImporter LoadFromAssetDatabase(string guid) => AssetDatabase.LoadAssetAtPath<SplashdownImporter>(AssetDatabase.GUIDToAssetPath(guid));
        
        public static void ValidateRegistry()
        {
            if (SplashdownRegistry == null)
            {
                SplashdownRegistry = new Dictionary<string, SplashdownImporter>();
            }

            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in assetPaths)
            {
                string extension = Path.GetExtension(assetPath);
                if (extension == ".splashdown")
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);

                    if (!SplashdownRegistry.ContainsKey(guid))
                    {
                        SplashdownRegistry[guid] = LoadFromAssetDatabase(guid);
                    }
                }
            }
        }
        
        private static SplashdownImporter FindByName(string targetName)
        {
            foreach (var kvp in SplashdownRegistry)
            {
                var splashdown = kvp.Value;
                if (splashdown.name == targetName)
                {
                    return splashdown;
                }
            }
            return null;
        }

        public static void SetSplash(string targetName)
        {
            var splashdown = FindByName(targetName);
            if (splashdown != null)
            {
                var handler = new LogoHandler(splashdown);
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
            if (splashdown != null)
            {
                var handler = new LogoHandler(splashdown);
                handler.RemoveSplash();
            }
            else
            {
                Debug.LogError($"{targetName} is not a known splashdown");
            }
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