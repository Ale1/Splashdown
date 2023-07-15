using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Splashdown.Editor
{
    public class LogoHandler
    {
        private Sprite sprite;
        private float splashTime;
        
        public LogoHandler(SplashdownImporter splashdown)
        {
            sprite = splashdown.Sprite;
            splashTime = splashdown.Options.SplashTime;
        }
        
        private bool HasLogo()
        {
            var existingLogos = PlayerSettings.SplashScreen.logos;
            foreach (var entry in existingLogos)
            {
                if (entry.logo == sprite)
                    return true;
            }

            return false;
        }

        public bool SetSplash()
        {
            if (sprite == null)
            {
                Debug.LogError("could not find splash logo");
                return false;
            }

            var allLogos = PlayerSettings.SplashScreen.logos;
            if (!HasLogo())
            {
                var splashdownLogo = PlayerSettings.SplashScreenLogo.Create(splashTime, sprite);
                allLogos = PushToArray(allLogos, splashdownLogo);
                PlayerSettings.SplashScreen.logos = allLogos;
                return true;
            }

            return false;
        }

        public bool RemoveSplash()
        {
            var AllLogos = PlayerSettings.SplashScreen.logos;
            if (HasLogo())
            {
                var list = AllLogos.ToList();
                list.RemoveAll(x => x.logo == sprite);
                PlayerSettings.SplashScreen.logos = list.ToArray();
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Push an item to index 0 of an array,
        /// </summary>
        /// <returns> returns a copy with modifications</returns>
        private static T[] PushToArray<T>(T[] originalArray, T newItem)
        {
            T[] newArray = new T[originalArray.Length + 1];
            newArray[0] = newItem;
            Array.Copy(originalArray, 0, newArray, 1, originalArray.Length);
            return newArray;
        }
    }
}
