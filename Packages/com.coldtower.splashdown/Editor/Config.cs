using System;
using System.IO;
using UnityEngine;

namespace Splashdown
{
    public static class Config
    {
        public static bool enabled => replaceSplash || replaceIcon;
        public static bool replaceSplash = false;
        public static bool replaceIcon = false;

        public static Color backgroundColor = Color.black;
        public static Color textColor = new (1f, 1f, 0.6f, 1f); //faded yellow

        public static string line1 = "Hello";
        public static string line2 = "World";
        public static string line3 = DateTime.Today.ToString("dd/MM/yy");

        public static bool logging = false;

        public static float splashTime = 4f;

        public static int PreBuildCallbackOrder = 9999;
        public const int PostbuildCallbackOrder = 1;

        public static string Path = "generatedSprite.png";
    }

    public static class ConfigUtil
    {
        
        public static string AssetsPath = Path.Combine("Assets", Config.Path);
        public static bool hasLine1 => !string.IsNullOrEmpty(Config.line1);
        public static bool hasLine2 => !string.IsNullOrEmpty(Config.line2);
        public static bool hasLine3 => !string.IsNullOrEmpty(Config.line3);
        public static int LineCount => (hasLine1 ? 1 : 0) + (hasLine2 ? 1 : 0) + (hasLine3 ? 1 : 0);
    }

}