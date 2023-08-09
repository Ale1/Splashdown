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
        public static int DefaultFontSize => DefaultFont.fontSize;
        

        [HideInInspector]
        public string fileName;

        public SerializableColor backgroundColor;
        public SerializableColor textColor;
        
        [CanBeNull] public string line1 = null;
        [CanBeNull] public string line2 = null;
        [CanBeNull] public string line3 = null;
        

        public SerializableInt SplashTime; 
        
        public int LineCount => (line1 != null ? 1 : 0) + (line2 != null ? 1 : 0) + (line3 != null ? 1 : 0);
        
        
        public SerializableInt TargetFontSize;

        /// <summary>
        /// Used as a temporary cache to load custom fonts dropped in the inspector, and then reset to null.  
        /// </summary>
        /// <remarks>use the <see cref="Font"/> Property to actually fetch the font</remarks>
        [NonSerialized] public Font fontAsset;
        
        [HideInInspector] [CanBeNull] public string fontGUID = null;

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

        
        /// <summary>
        /// Used as a temporary cache to load background textures dropped in the inspector, and then reset to null.  
        /// </summary>
        /// <remarks>use the <see cref="BackgroundTexture"/> Property to actually fetch the backgroundTexture</remarks>
        [NonSerialized] public Texture2D backgroundTexture;
        
        [HideInInspector] [CanBeNull] public string backgroundTextureGuid = null;
        public Texture2D BackgroundTexture
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(backgroundTextureGuid);
                if(path == null)
                    Debug.LogError("font path is null");
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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
                    if (asset is Sprite sprite)
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
            SplashTime = other.SplashTime.hasValue ? other.SplashTime : SplashTime;
            fontGUID = other.fontGUID ?? fontGUID;
            TargetFontSize = other.TargetFontSize.hasValue ? other.TargetFontSize : TargetFontSize;
            backgroundTextureGuid = other.backgroundTextureGuid ?? backgroundTextureGuid;
        }
        
        public void ApplyDefaultValues()
        {
            fontGUID ??= DefaultFontGUID;
            
            if (!backgroundColor.hasValue)
                backgroundColor = Constants.DefaultBackgroundColor;

            if (!textColor.hasValue)
                textColor = Constants.DefaultTextColor;

            if (!SplashTime.hasValue)
                SplashTime = Constants.DefaultSplashtime;

            if (!TargetFontSize.hasValue)
                TargetFontSize = DefaultFontSize;
        }
    }
    
    
    /// We use Serializable Color for consistent handling of nullables
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
            return hasValue ? color : null;
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
    
    [Serializable]
    public struct SerializableInt
    {
        public int value;
        public bool hasValue;

        public SerializableInt(int value)
        {
            this.value = value;
            this.hasValue = true;
        }

        public int ToInt()
        {
            if (hasValue)
            {
                return Mathf.Clamp(value, Constants.MinFontSize, Constants.MaxFontSize);
            }
            else
            {
                Debug.LogError("Splashdown: fontSize is null");
                return Constants.MaxFontSize;
            }
        }

        public static implicit operator SerializableInt(int value)
        {
            return new SerializableInt(value);
        }

        public static implicit operator int?(SerializableInt serializableInt)
        {
            return serializableInt.ToInt();
        }
    }
}
