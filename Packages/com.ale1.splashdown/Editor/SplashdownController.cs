using System;
using System.Collections.Generic;
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
        private static PlayerSettings.SplashScreenLogo[] _backupLogos;
        private static Dictionary<PlatformIconKind,PlatformIcon[]> _backupIcons;

        private static bool restoreSplash;
        private static bool restoreIcons;

        private static SplashdownOptions config => MySplashdown.Options;
        private static Sprite sprite => MySplashdown.Sprite;

  
        private static SplashdownImporter _mySplashdown; //backing field.
        public static SplashdownImporter MySplashdown
        {
                get
                {
                    if (_mySplashdown != null)
                        return _mySplashdown;
                
                    string[] guids = AssetDatabase.FindAssets("");
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if(System.IO.Path.GetExtension(path) == ".splashdown")
                        {
                            _mySplashdown = (SplashdownImporter)AssetImporter.GetAtPath(path);
                        }
                    }
                    if(_mySplashdown == null)
                        Debug.LogError("no splashdown found");
                    return _mySplashdown;
                }
        }

        public static void Build(Sprite sprite, float splashTime)
        {
            if (config == null)
            {
                Debug.LogError("something went wrong");
                return;
            }

            if (sprite == null)
            {
                Debug.LogError("splashdown.Sprite is null");
                return;
            }
            
            if (config.useAsSplash)
            {
                restoreSplash = SetSplash(sprite, splashTime);
            }
            
            if (config.useAsAppIcon)
            {
                restoreIcons = SetIcons(sprite);
               
            }
        }
        

        public static void Restore()
        {
            if(restoreSplash) RestoreSplash();
            if(restoreIcons) RestoreIcons();
            AssetDatabase.SaveAssets();
        }
        

        private static bool SetSplash(Sprite sprite, float splashTime)
        {
            PlayerSettings.SplashScreenLogo[] backupLogos = PlayerSettings.SplashScreen.logos;
            
            if (sprite == null)
            {
                Debug.LogError("could not find splash logo");
                return false;
            }
            
            var splashdownLogo = PlayerSettings.SplashScreenLogo.Create(splashTime, sprite);

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

        private static void RestoreSplash()
        {
            PlayerSettings.SplashScreen.logos = _backupLogos;
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