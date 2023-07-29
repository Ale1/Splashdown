
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

        
        public static void SetSplash(string guid)
        {
            var splashdownData = SplashdownUtils.GetOptionsFromGuid(guid);
            if (splashdownData != null)
            {
                var handler = new LogoHandler(splashdownData);
                handler.SetSplash();
            }
        }
        
        public static void RemoveSplash(string guid)
        {
            var splashdownData = SplashdownUtils.GetOptionsFromGuid(guid);
            
            if (splashdownData == null)
                return;
            var handler = new LogoHandler(splashdownData);
            handler.RemoveSplash();
        }

        
        public static void SetIcons(string guid)
        {
            if (iconHandler != null)
            {
                iconHandler.RestoreIcons();
                iconHandler.Dispose();
                iconHandler = null;
            }

            var splashdownData = SplashdownUtils.GetOptionsFromGuid(guid);
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
                //Debug.Log("restoring icons");
                iconHandler.RestoreIcons();
                iconHandler.Dispose();
                iconHandler = null;
            }
        }
        
        
    }
}