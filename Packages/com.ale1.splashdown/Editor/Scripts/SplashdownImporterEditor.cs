
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Splashdown.Editor
{
    [CustomEditor(typeof(SplashdownImporter))]
    public class SplashdownImporterEditor : ScriptedImporterEditor
    {
        private Options options;
        
        public override void OnInspectorGUI()
        {
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
       
            

            EditorGUILayout.Space(30);
            DrawDefaultInspectorWithoutScript();
            
            //during reimport options can be null for a frame before inspector fetches new options, breaking inspector. 
            if (options == null)
                return;
            
            DrawDivider();
            
            // Draw the Options fields
            EditorGUI.BeginChangeCheck();
            //todo: show warning when these inspector options will be overriden by dynamic options. e.g: "if(importer.dynamicOptions && dynamicOptions.hasLine1) => |show warning|"
            options.line1 = EditorGUILayout.TextField("Line 1", options.line1 ?? null);
            options.line2 = EditorGUILayout.TextField("Line 2", options.line2 ?? null);
            options.line3 = EditorGUILayout.TextField("Line 3", options.line3 ?? null);
            options.backgroundColor = EditorGUILayout.ColorField("Background Color", (UnityEngine.Color) options.backgroundColor);
            options.textColor = EditorGUILayout.ColorField("Text Color Color", (UnityEngine.Color) options.textColor);

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


        private void DrawDefaultInspectorWithoutScript()
        {
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip "m_Script" property
                if(iterator.propertyPath == "m_Script") continue;
                EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
            }
            serializedObject.ApplyModifiedProperties();
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
        
        void DrawDivider(int space = 10)
        {
            EditorGUILayout.Space(space);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(space);
        }
        
        
    }
}