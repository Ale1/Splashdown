using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Splashdown.Editor
{
    [CustomEditor(typeof(SplashdownImporter))]
    public class SplashdownImporterEditor : ScriptedImporterEditor
    {
        private Options options;

        private GUIStyle _line1Style;
        private GUIStyle _line2Style;
        private GUIStyle _line3Style;
        private GUIStyle _backgroundStyle;
        private GUIStyle _textcolorStyle;

        private Options _dynamicOptions;
        private bool _initialized;

        private void ResetStyles()
        {
            _line1Style = null;
            _line2Style = null;
            _line3Style = null;
            _backgroundStyle = null;
            _textcolorStyle = null;
        }
        
        private void SetStyles()
        {
            Color defaultLabelColor = SplashdownStyles.DefaultLabelColor;
            Color warningColor = SplashdownStyles.DynamicLabelColor;
            var importer = (SplashdownImporter)target;
            bool useDynamic = importer.useDynamicOptions;
            
            if(useDynamic)
                _dynamicOptions = importer.FetchDynamicOptions();

            _line1Style = new GUIStyle(SplashdownStyles.TemplateLabelStyle) 
            {
                normal = { textColor = useDynamic && _dynamicOptions?.line1 != null ? warningColor : defaultLabelColor }
            };

            _line2Style = new GUIStyle(SplashdownStyles.TemplateLabelStyle)
            {
                normal = { textColor = useDynamic && _dynamicOptions?.line2 != null ? warningColor : defaultLabelColor }
            };
            
            _line3Style = new GUIStyle(SplashdownStyles.TemplateLabelStyle)
            {
                normal = { textColor = useDynamic && _dynamicOptions?.line3 != null ? warningColor: defaultLabelColor }
            };

            _backgroundStyle = new GUIStyle(SplashdownStyles.TemplateLabelStyle)
            {
                normal = 
                {
                    textColor = useDynamic && _dynamicOptions is { backgroundColor: { hasValue: true } }
                        ? warningColor
                        : defaultLabelColor
                }
            };

            _textcolorStyle = new GUIStyle(SplashdownStyles.TemplateLabelStyle)
            {
                normal =
                {
                    textColor = useDynamic && _dynamicOptions is { textColor: { hasValue: true } }
                        ? warningColor
                        : defaultLabelColor
                }
            };
        }

        public override void OnInspectorGUI()
        {
            if(_line1Style == null)
                SetStyles();
            
            var importer = (SplashdownImporter)target;
            
            if(options == null)
                options = SplashdownImporter.DeserializeOptions(importer.assetPath);

            GUIStyle centered = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            
            EditorGUILayout.Space(10);

            // Saving current GUI state
            bool prevState = GUI.enabled;
            var originalColor = GUI.backgroundColor;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("STATUS", centered, GUILayout.Width(286));

            EditorGUILayout.BeginHorizontal();
            
            DrawVerticalLine();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(134));
            DrawSplashActivationButtons(importer, originalColor);
            // Restoring original GUI state
            GUI.enabled = prevState;
            EditorGUILayout.EndVertical();
            
            DrawVerticalLine();

            EditorGUILayout.BeginVertical(GUILayout.Width(134));
            DrawIconActivationButtons(importer, originalColor);
            GUI.enabled = prevState;
            EditorGUILayout.EndVertical();
            
            DrawVerticalLine();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(24);

            //during reimport options can be null for a frame before inspector fetches new options, breaking inspector. 
            if (options == null)
                return;
            
            DrawDivider();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Use Dynamic Options", ""), EditorStyles.boldLabel, GUILayout.Width(150));
            var toggle = EditorGUILayout.Toggle(importer.useDynamicOptions);
            if (toggle != importer.useDynamicOptions)
            {
                importer.useDynamicOptions = toggle;
                EditorUtility.SetDirty(importer);
                ResetStyles();
                return;
            }
            EditorGUILayout.EndHorizontal();
            

            EditorGUI.BeginChangeCheck();
            
            if (toggle && _dynamicOptions != null)
            {
                EditorGUILayout.Space(6);
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox($"some values overriden by: {_dynamicOptions.source}", MessageType.Info);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(12);
            }
            
            // Draw the Options fields

            DrawLabelWithText(ref options.line1, "Line 1", _dynamicOptions?.line1, _line1Style);
            DrawLabelWithText(ref options.line2, "Line 2", _dynamicOptions?.line2, _line2Style);
            DrawLabelWithText(ref options.line3, "Line 3", _dynamicOptions?.line3, _line3Style);
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Background Color", _dynamicOptions?.backgroundColor.ToString()), _backgroundStyle, GUILayout.Width(128));
            options.backgroundColor = EditorGUILayout.ColorField((Color) options.backgroundColor, GUILayout.MaxWidth(84));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Text Color",_dynamicOptions?.backgroundColor.ToString()), _textcolorStyle, GUILayout.Width(128));
            options.textColor = EditorGUILayout.ColorField((Color) options.textColor, GUILayout.MaxWidth(84));
            GUILayout.EndHorizontal();
            
            
            if (EditorGUI.EndChangeCheck())
            {
                importer.inspectorOptions = options;
                EditorUtility.SetDirty(importer);
            }
            EditorGUILayout.Space(20);
            
            DrawDivider();
            
            EditorGUI.BeginChangeCheck();

            if (!options.TargetFontSize.hasValue)
                options.TargetFontSize = Constants.MaxFontSize;
            
            options.TargetFontSize = EditorGUILayout.IntSlider(new GUIContent("Font Size"), (int) options.TargetFontSize, Constants.MinFontSize, Constants.MaxFontSize);
            
            if (!string.IsNullOrEmpty(options.fontGUID))
            {
                string path = AssetDatabase.GUIDToAssetPath(options.fontGUID);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("Using Font:", path);
                EditorGUI.EndDisabledGroup();
                
                options.fontAsset = (Font)EditorGUILayout.ObjectField("Font Selector", options.fontAsset, typeof(Font), false);
            }
            else
            {
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("Using Font:", $"Splashdown Default ({Constants.DefaultFontName})");
                EditorGUI.EndDisabledGroup();
                
                options.fontAsset = (Font)EditorGUILayout.ObjectField("Font Selector", options.fontAsset, typeof(Font), false);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (options.fontAsset != null)
                {
                    string fontPath = AssetDatabase.GetAssetPath(options.fontAsset);
                    options.fontGUID = AssetDatabase.AssetPathToGUID(fontPath);
                }
                else
                {
                    options.fontGUID = string.Empty;
                }

                importer.inspectorOptions = options;
                EditorUtility.SetDirty(importer);
            }
            
            DrawDivider();
            //Check for background Texture
            EditorGUI.BeginChangeCheck();
            
            
            options.backgroundTexture = (Texture2D)EditorGUILayout.ObjectField("background Texture:", options.BackgroundTexture, typeof(Texture2D), false);
                
            if(options.backgroundTexture != null && (options.backgroundTexture.width != Constants.DefaultWidth || options.backgroundTexture.height != Constants.DefaultHeight))
            {
                
                EditorGUILayout.LabelField($"For best results use a texture of size: {Constants.DefaultWidth} x {Constants.DefaultHeight}");
                EditorGUILayout.LabelField($"(Current is {options.backgroundTexture.width} x {options.backgroundTexture.height})");
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                if (options.backgroundTexture != null)
                {
                    string texturePath = AssetDatabase.GetAssetPath(options.backgroundTexture);
                        options.backgroundTextureGuid = AssetDatabase.AssetPathToGUID(texturePath); 
                }
                else
                {
                    options.backgroundTextureGuid = string.Empty;
                }
                
                importer.inspectorOptions = options;
                EditorUtility.SetDirty(importer);
            }

            DrawDivider();
            
            ApplyRevertGUI();
        }

        private void ForceReimport(SplashdownImporter importer)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(importer));
            options = null;
        }
        
        
        private void DrawLabelWithText(ref string optionValue, string label, string tooltip, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), style, GUILayout.Width(128));
            optionValue = EditorGUILayout.TextField(optionValue, GUILayout.MaxWidth(84));
            GUILayout.EndHorizontal();
        }
        
        private void DrawSplashActivationButtons(SplashdownImporter importer, Color originalColor)
        {
            Color defaultColor = GUI.color;
            GUI.color = importer.IsSplashActive ? Color.green : Color.red;
            GUILayout.Label(importer.IsSplashActive ? " ACTIVE SPLASH " : "INACTIVE SPLASH", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            GUI.color = defaultColor;  // Reset color back to default

            EditorGUILayout.Space(10);
                
            Color splashButtonColor = EditorGUIUtility.isProSkin ? Color.cyan : Color.blue;
                
            // Setting up for the "Activate Splash" button
            GUI.enabled = !importer.IsSplashActive;
            GUI.backgroundColor = importer.IsSplashActive ? Color.grey : splashButtonColor;
            if (GUILayout.Button("Activate Splash",GUILayout.Width(122), GUILayout.Height(32)))
            {
                importer.SetActiveSplash(true);
                EditorUtility.SetDirty(importer);
                
                // Defer the reimport action until the next frame
                EditorApplication.delayCall += () =>
                {
                    ForceReimport(importer); // force save without need to hit Apply button
                    SplashdownController.SetSplash(importer.GetGuid);
                };
            }

            // Restoring GUI state, then setting up for the "Deactivate" button
            GUI.backgroundColor = originalColor;
            GUI.enabled = importer.IsSplashActive;
            GUI.backgroundColor = importer.IsSplashActive ? splashButtonColor : Color.grey;
            if (GUILayout.Button("Deactivate Splash",GUILayout.Width(122), GUILayout.Height(32)))
            {
                importer.SetActiveSplash(false);
                EditorUtility.SetDirty(importer);
                EditorApplication.delayCall += () =>
                {
                    ForceReimport(importer);
                    SplashdownController.RemoveSplash(importer.GetGuid);
                };
            }
            GUI.backgroundColor = originalColor;
        }

        private void DrawIconActivationButtons(SplashdownImporter importer, Color originalColor)
        {
            Color defaultColor = GUI.color;
            GUI.color = importer.IsIconActive ? Color.green : Color.red;
            GUILayout.Label(importer.IsIconActive ? "  ACTIVE ICON  " : " INACTIVE ICON ", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter});
            GUI.color = defaultColor;      // Reset color back to default
                
            EditorGUILayout.Space(10);
                
            Color IconButtonColor = EditorGUIUtility.isProSkin ? Color.cyan : Color.blue;

            // Setup for the "Activate Icon" button
            GUI.backgroundColor = originalColor;
            GUI.enabled = !importer.IsIconActive;
            GUI.backgroundColor = importer.IsIconActive ? Color.grey : IconButtonColor;
            if (GUILayout.Button("Activate Icon",GUILayout.Width(122), GUILayout.Height(32)))
            {
                importer.SetActiveIconWithEvent(true);
                EditorUtility.SetDirty(importer);
                EditorApplication.delayCall += () =>
                {
                    ForceReimport(importer); // force save without need to hit Apply button
                };
            }

            // Restoring GUI state, then setting up for the "Deactivate" button
            GUI.backgroundColor = originalColor;
            GUI.enabled = importer.IsIconActive;
            GUI.backgroundColor = importer.IsIconActive ? IconButtonColor : Color.grey;
            if (GUILayout.Button("Deactivate Icon",GUILayout.Width(122), GUILayout.Height(32)))
            {
                importer.SetActiveIconWithEvent(false);
                EditorUtility.SetDirty(importer);
                EditorApplication.delayCall += () =>
                {
                    ForceReimport(importer);
                };
            }
            // Restoring original GUI state
            GUI.backgroundColor = originalColor;
        }

        private void DrawVerticalLine()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(108), GUILayout.Width(6));
            rect.width = 3;
            EditorGUI.DrawRect(rect, Color.gray);
        }
        
        private void DrawDivider(int space = 10)
        {
            EditorGUILayout.Space(space);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(space);
        }
        
        
    }
}