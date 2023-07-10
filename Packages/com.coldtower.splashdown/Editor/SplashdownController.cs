using System;
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
        
        
        [MenuItem("Assets/Create/Splashdown/Icon")]
        public static void GenerateIcon()
        {
            string targetDirectory = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(targetDirectory))
            {
                targetDirectory = "Assets";
            }
            else if (Path.GetExtension(targetDirectory) != "") 
            {
                targetDirectory = targetDirectory.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            

            var path = Path.Combine(targetDirectory, Config.filename);
            var sprite = SpriteGenerator.Generate(path);
  
            if (sprite != null)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite));
                EditorPrefs.SetString("Splashdown/logoGUID",guid);
                EditorPrefs.SetString("Splashdown/iconGUID", guid);
            }
        }

        public static void Build()
        {
            if (Config.replaceSplash)
            { 
                //reuse old path if possible to override generatedSprite.
                var lastSprite = GetGUIDFromPrefs("Splashdown/logoGUID");
                var oldPath = AssetDatabase.GUIDToAssetPath(lastSprite);
                GenerateIcon(oldPath);
                sprite = FetchSpriteFromPrefs("Splashdown/logoGUID");
                var shouldRestore = SetSplash(sprite);
                restoreSplash = shouldRestore;
            }
            
            if (Config.replaceIcon)
            {
                var sprite = FetchSpriteFromPrefs("Splashdown/iconGUID");
                if (sprite == null)
                {
                    GenerateIcon();
                    sprite = FetchSpriteFromPrefs("Splashdown/iconGUID");
                }
                var shouldRestore = SetIcons(sprite);
                restoreIcons = shouldRestore;
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
            var transparentBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.coldtower.splashdown/Editor/splashdown_transparent.png");

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
            if(Config.replaceSplash)
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

        /// <summary>
        /// Remove the first item from the array
        /// </summary>
        /// <returns> returns a copy with (length - 1) </returns>
        private static T[] RemoveFirst<T>(this T[] originalArray)
        {
            T[] newArray = new T[originalArray.Length - 1];
            Array.Copy(originalArray, 1, newArray, 0, originalArray.Length - 1);
            return newArray;
        }
    }
}