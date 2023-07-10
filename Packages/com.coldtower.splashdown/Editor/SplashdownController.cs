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
        private static PlayerSettings.SplashScreenLogo[] backupLogos;
        private static Dictionary<PlatformIconKind,PlatformIcon[]> backupIcons;
        
        [MenuItem("Assets/Create/Splashdown/Icon")]
        public static void Generate()
        {
            
            string targetDirectory = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Path.GetExtension(targetDirectory) != "") 
            {
                targetDirectory = targetDirectory.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            var path = Path.Combine(targetDirectory, Config.filename);
            var sprite = SpriteGenerator.Generate(path);
  
            if (sprite != null)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite));
                
                //EditorPrefs.SetString("Splashdown/spriteGUID",guid);
                
            }
        }
        
        public static void Run()
        {
            var sprite = SpriteGenerator.Generate(Path.Combine("Assets", Config.filename));
            
            if (sprite == null)
            {
                if(Config.logging) Debug.LogError("could not find asset");
                return;
            }
            
            if(Config.replaceSplash) SetSplash(sprite);
            if(Config.replaceIcon) SetIcons(sprite);
        }


        public static void Restore()
        {
            RestoreLogos();
            RestoreIcons();
        }

        private static void SetSplash(Sprite sprite)
        {
            var splashdownLogo =
                    PlayerSettings.SplashScreenLogo.Create(Config.splashTime, sprite); 
                PlayerSettings.SplashScreenLogo[] backupLogos = PlayerSettings.SplashScreen.logos;

                if (!backupLogos.Contains(splashdownLogo))
                {
                    var tempLogos = backupLogos.PushToArray(splashdownLogo);
                    PlayerSettings.SplashScreen.logos = tempLogos;
                }
        }


        private static void SetIcons(Sprite sprite)
        {
            var transparentBackground = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.coldtower.splashdown/Editor/splashdown_transparent.png");
            
                if (FetchPlatform(out NamedBuildTarget platform) && FetchIconKind(out PlatformIconKind[] kinds))
                {
                    foreach (var kind in kinds)
                    {
                        var icons = PlayerSettings.GetPlatformIcons(platform, kind);
                        backupIcons[kind] = icons;

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
                }
        }

        private static void RestoreLogos()
        {
            if(Config.replaceIcon)
                PlayerSettings.SplashScreen.logos = backupLogos;
        }

        private static void RestoreIcons()
        {
            if (Config.replaceSplash)
            {
                if (FetchPlatform(out NamedBuildTarget platform))
                {
                    foreach (var entry in backupIcons)
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