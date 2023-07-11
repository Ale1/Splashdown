using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.iOS;
using UnityEngine;


namespace Splashdown
{
    public static class SplashdownController
    {
        private static PlayerSettings.SplashScreenLogo[] _backupLogos;
        private static Dictionary<PlatformIconKind,PlatformIcon[]> _backupIcons;

        private static bool restoreSplash;
        private static bool restoreIcons;
        
        const string defaultFilename = "MySplashdown.splashdown"; //todo: move to constants
        
        [MenuItem("Assets/Create/New Splashdown")]
        public static void CreateSplashdown()
        {
            CreateSplashdown(AssetDatabase.GetAssetPath(Selection.activeObject));
        }
        
        private static void CreateSplashdown(string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = "Assets";
            }
            else if (Directory.Exists(targetPath)) // its a directory.
            {
                targetPath = Path.Combine(targetPath, defaultFilename);
            }
            else if (Path.GetExtension(targetPath) == ".splashdown")
            {
                // keep path as it is, to replace file.
            }
            else if (Path.GetExtension(targetPath) != "") //path is pointing to a non-png file.  Use same location but use default filename.
            {
                targetPath = targetPath.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                targetPath = Path.Combine(targetPath, defaultFilename);
            }

            SplashdownGenerator.GenerateSplashdownFile(targetPath);
            
        }

        [MenuItem("Splashdown/Test")]
        public static void Build()
        {
            var splashdown = Config.MySplashdown;
            if (splashdown == null)
            {
                Debug.LogError("something went wrong");
                return;
            }
            
            Debug.Log("hello" + AssetDatabase.GetAssetPath(splashdown.Sprite));

            if (splashdown.Sprite == null)
            {
                Debug.LogError("splashdown.Sprite is null");
                return;
            }
            
            if (Config.includeSplash)
            {
                restoreSplash = SetSplash(splashdown.Sprite);
            }
            
            if (Config.replaceIcon)
            {
                restoreIcons = SetIcons(splashdown.Sprite);
               
            }
        }



        public static void Restore()
        {
            if(restoreSplash) RestoreSplash();
            if(restoreIcons) RestoreIcons();
            AssetDatabase.SaveAssets();
        }

        private static Sprite FetchSpriteFromPrefs(string key)
        {
            var spriteGUID = GetGUIDFromPrefs(key);
            if (spriteGUID != "")
            {
                var path = AssetDatabase.GUIDToAssetPath(spriteGUID);
                if (path != "")
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    return sprite;   
                }
            }
            return null;
        }

        private static string GetGUIDFromPrefs(string key)
        {
            return EditorPrefs.GetString(key, "");
        }

        private static bool SetSplash(Sprite sprite)
        {
            PlayerSettings.SplashScreenLogo[] backupLogos = PlayerSettings.SplashScreen.logos;
            
            if (sprite == null)
            {
                if(Config.logging) Debug.LogError("could not find splash logo");
                return false;
            }
            
            var splashdownLogo = PlayerSettings.SplashScreenLogo.Create(Config.splashTime, sprite);

            if (!backupLogos.Contains(splashdownLogo))
            {
                var tempLogos = backupLogos.PushToArray(splashdownLogo);
                PlayerSettings.SplashScreen.logos = tempLogos;
            }
            return true;
        }


        private static bool SetIcons(Sprite sprite)
        {
            var transparentBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.ale1.splashdown/Editor/splashdown_transparent.png");  //todo: move to constants

            if (sprite == null || transparentBackground == null)
            {
                if(Config.logging) Debug.LogError("error loading sprites");
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

        private static void RestoreSplash()
        {
            if(Config.includeSplash)
                PlayerSettings.SplashScreen.logos = _backupLogos;
        }

        private static void RestoreIcons()
        {
            if (Config.replaceIcon)
            {
                if (FetchPlatform(out NamedBuildTarget platform))
                {
                    foreach (var entry in _backupIcons)
                    {
                        PlayerSettings.SetPlatformIcons(platform, entry.Key, entry.Value);
                    }
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
                    if(Config.logging) Debug.LogError("unsupported platform");
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
                    if(Config.logging) Debug.LogWarning("unsupported build target");
                    return false;
            }
        }


        /// <summary>
        /// Push an item to index 0 of an array,
        /// </summary>
        /// <returns> returns a copy with modifications</returns>
        private static T[] PushToArray<T>(this T[] originalArray, T newItem)
        {
            T[] newArray = new T[originalArray.Length + 1];
            newArray[0] = newItem;
            Array.Copy(originalArray, 0, newArray, 1, originalArray.Length);
            return newArray;
        }
    }
}