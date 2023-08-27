using UnityEditor;
using UnityEngine;

namespace Splashdown.Editor
{
    public static class SplashdownStyles
    {
        public static Color DynamicLabelColor = Color.yellow;  
        
        private static Color? _defaultLabelColor;
        public static Color DefaultLabelColor
            {
                get
                {
                    if (!_defaultLabelColor.HasValue)
                    {
                        _defaultLabelColor = EditorStyles.label.normal.textColor;
                    }
                    return _defaultLabelColor.Value;
                }
            }

        private static GUIStyle _templateLabelStyle;
        public static GUIStyle TemplateLabelStyle
        {
            get
            {
                if (_templateLabelStyle == null)
                {
                    _templateLabelStyle = new GUIStyle(EditorStyles.label);
                }
                return _templateLabelStyle;
            }
        }

          
    }
}

