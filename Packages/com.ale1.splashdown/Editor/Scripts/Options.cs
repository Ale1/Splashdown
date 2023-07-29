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
        
        public static Font DefaultFont => AssetDatabase.LoadAssetAtPath<Font>(Constants.FontPath_Roboto);
        public static string DefaultFontGUID => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(DefaultFont));
        
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
        
        [HideInInspector] [CanBeNull] public string fontGUID = null;
        
        /// <summary>
        /// Used as a temporary cache to load custom fonts dropped in the inspect, and then set to null.  
        /// </summary>
        /// <remarks>use the <see cref="Font"/> Property to actually fetch the font</remarks>
        [HideInInspector] public Font fontAsset;

        public Font Font
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
                    if (asset is Sprite sprite && sprite.name == Constants.GeneratedSpriteName)
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
                backgroundColor = Constants.DefaultBackgroundColor;

            if (!textColor.hasValue)
                textColor = Constants.DefaultTextColor;

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
