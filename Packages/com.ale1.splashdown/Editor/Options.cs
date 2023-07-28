using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Splashdown.Editor
{
    [Serializable]
    public class Options
    {
        public Options(bool applyDefaultValues = false)
        {
            if(applyDefaultValues)
                ApplyDefaultValues();
        }
        
        public static Font DefaultFont => AssetDatabase.LoadAssetAtPath<Font>("Packages/com.Ale1.splashdown/Editor/Splashdown_RobotoMono.ttf");
        public static string DefaultFontGUID => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(DefaultFont));

        public static Color DefaultBackground => Color.black;
        public static Color DefaultTextColor => new Color(1f, 1f, 0.6f, 1f);
        public static float DefaultSplashtime => 4f;
        
        [HideInInspector]
        public string fileName;

        public SerializableColor backgroundColor;
        public SerializableColor textColor;

        [CanBeNull] public string line1 = null;
        [CanBeNull] public string line2 = null;
        [CanBeNull] public string line3 = null;
        

        public float? SplashTime; 
        
        public int LineCount => (line1 != null ? 1 : 0) + (line2 != null ? 1 : 0) + (line3 != null ? 1 : 0);
        
        public static int PreBuildCallbackOrder => 9999;
        public const int PostbuildCallbackOrder = 1;
        
    
        [HideInInspector] [CanBeNull] public string fontGUID = null;
        
        [HideInInspector] public Font fontAsset;

        public Font font
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(fontGUID);
                if(path == null)
                    Debug.LogError("font path is null");
                return AssetDatabase.LoadAssetAtPath<Font>(path);
            }
        }
        
        
        [HideInInspector]
        public string assetPath;

        public Sprite Sprite
        {
            get
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                
                foreach (var asset in assets)
                {
                    if (asset is Sprite sprite && sprite.name == "Generated")
                    {
                        return sprite;
                    }
                }

                throw new Exception("empty sprite");
            }
        }

        public void UpdateWith(Options other)
        {
            line1 = other.line1 ?? line1;
            line2 = other.line2 ?? line2;
            line3 = other.line3 ?? line3;
            backgroundColor = other.backgroundColor.hasValue ? other.backgroundColor : backgroundColor;
            textColor = other.textColor.hasValue ? other.textColor : textColor;
            SplashTime = other.SplashTime ?? SplashTime;
        }
        
        public void ApplyDefaultValues()
        {
            if (fontGUID == null)
            {
                fontGUID = DefaultFontGUID;
            }
            
            if (!backgroundColor.hasValue)
                backgroundColor = DefaultBackground;

            if (!textColor.hasValue)
                textColor = DefaultTextColor;

            if (SplashTime == null)
                SplashTime = DefaultSplashtime;
        }
    }
    
    [Serializable]
    public struct SerializableColor
    {
        public Color color;
        public bool hasValue;

        public SerializableColor(Color color)
        {
            this.color = color;
            this.hasValue = true;
        }

        public Color? ToColor()
        {
            return hasValue ? color : (Color?)null;
        }

        public static implicit operator SerializableColor(Color color)
        {
            return new SerializableColor(color);
        }

        public static implicit operator Color?(SerializableColor serializableColor)
        {
            return serializableColor.ToColor();
        }
    }
}
