using System;

using UnityEngine;

namespace Splashdown
{
    [Serializable]
    public class Options
    {
        public bool useAsSplash;
        public bool useAsAppIcon;
        public bool logging;
        public Color backgroundColor = Color.black;
        public Color textColor = new Color(1f, 1f, 0.6f, 1f);

        public string line1;
        public string line2;
        public string line3;
        
        
        public float SplashTime => 4f; //todo
        
        public bool active => useAsSplash || useAsAppIcon;
        public bool hasLine1 => !string.IsNullOrEmpty(line1);
        public bool hasLine2 => !string.IsNullOrEmpty(line2);
        public bool hasLine3 => !string.IsNullOrEmpty(line3);
        public int LineCount => (hasLine1 ? 1 : 0) + (hasLine2 ? 1 : 0) + (hasLine3 ? 1 : 0);
        
        public static int PreBuildCallbackOrder => 9999;
        public const int PostbuildCallbackOrder = 1;
    }
}
