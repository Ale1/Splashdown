

using UnityEngine;

namespace Splashdown
{
    public static class Constants
    {
        public const string PackageName = "com.ale1.Splashdown";
        public static string EditorPrefsKey => PackageName;
        public const string DefaultFontName = "Roboto Mono";
        public const string FontPath_Roboto = "Packages/com.ale1.splashdown/Editor/Fonts/Splashdown_RobotoMono.ttf";
        public const string GeneratedSpriteName = "GeneratedSprite";
        public const string GeneratedOptionsName = "Options";
        public const string SplashdownFileType = "splashdown";
        public const string SplashdownExtension = "."+ SplashdownFileType;
        public static Color DefaultBackgroundColor = Color.black;
        public static Color DefaultTextColor = new (1f, 1f, 0.6f, 1f);
    }
}
