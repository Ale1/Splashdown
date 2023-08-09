using UnityEngine;

namespace Splashdown
{
    public static class Constants
    {
        public const string PackageName = "com.ale1.Splashdown";
        public static string EditorPrefsKey => PackageName;
        public const string DefaultFontName = "Roboto Mono";
        public const string FontPath_Roboto = "Packages/com.ale1.splashdown/Editor/Fonts/Splashdown_RobotoMono.ttf";
        public const string FontPath_NanumGothic = "Packages/com.ale1.splashdown/Editor/Fonts/Splashdown_NanumGothicCoding.ttf";
        public const string GeneratedSpriteName = "GeneratedSprite";
        public const string GeneratedOptionsName = "Options";
        public const string SplashdownFileType = "splashdown";
        public const string SplashdownExtension = "."+ SplashdownFileType;
        public static Color DefaultBackgroundColor = Color.black;
        public static Color DefaultTextColor = new (1f, 1f, 0.6f, 1f);
        public const int MinFontSize = 24;
        public const int MaxFontSize = 68;
        public const int DefaultWidth = 360;
        public const int DefaultHeight = 360;
        public const int DefaultSplashtime = 4; //seconds


    }
}
