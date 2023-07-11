using System;
using UnityEditor;
using UnityEngine;

namespace Splashdown
{
    public static class Config
    {
        public static bool isDirty = false;
        
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

        
        public static bool active => includeSplash || replaceIcon;
        public static bool includeSplash => MySplashdown.includeInSplash;
        public static bool replaceIcon => MySplashdown != null && MySplashdown.includeInAppIcon;

        public static Color backgroundColor => MySplashdown.backgroundColor;
        public static Color textColor => MySplashdown.textColor; 

        public static string line1
        {
            get => MySplashdown.line1;
            set
            {
                if (value != MySplashdown.line1) isDirty = true;
                MySplashdown.line1 = value;
            } 
        }

        public static string line2
        {
            get => MySplashdown.line2;
            set
            {
                if (value != MySplashdown.line2) isDirty = true;
                MySplashdown.line2 = value;
            }
        }

        public static string line3
        {
            get => MySplashdown.line3;
            set
            {
                if (value != MySplashdown.line3) isDirty = true;
                MySplashdown.line3 = value;
            } 
        }

        public static bool logging => MySplashdown.enableLogging;
        public static float splashTime => 4f; //todo

        public static int PreBuildCallbackOrder => 9999;
        public const int PostbuildCallbackOrder = 1;
        
        public static bool hasLine1 => !string.IsNullOrEmpty(Config.line1);
        public static bool hasLine2 => !string.IsNullOrEmpty(Config.line2);
        public static bool hasLine3 => !string.IsNullOrEmpty(Config.line3);
        public static int LineCount => (hasLine1 ? 1 : 0) + (hasLine2 ? 1 : 0) + (hasLine3 ? 1 : 0);
    }
    

}