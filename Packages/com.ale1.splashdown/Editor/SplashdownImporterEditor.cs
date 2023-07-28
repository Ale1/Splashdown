
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
            EditorGUILayout.LabelField("STATUS", centered, GUILayout.Width(268));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(134));

            DrawSplashActivationButtons(importer, originalColor);

            // Restoring original GUI state
            GUI.enabled = prevState;
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(134));

            DrawIconActivationButtons(importer, originalColor);
            GUI.enabled = prevState;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(20);

            EditorGUILayout.EndHorizontal();
            
            
            EditorGUILayout.Space(40);
            DrawDefaultInspectorWithoutScript();
            
            //during reimport options can be null for a frame before inspector fetches new options, breaking inspector. 
            if (options == null)
                return;
            
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
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(40);

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
                    string assetPath = AssetDatabase.GetAssetPath(options.fontAsset);
                    options.fontGUID = AssetDatabase.AssetPathToGUID(assetPath);
                    options.fontAsset = null; // we clear it immediately after getting the GUID
                }
                else
                {
                    options.fontGUID = string.Empty;
                }

                importer.inspectorOptions = options;
                EditorUtility.SetDirty(importer);
            }
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
                GUI.color = importer.ActiveSplash ? Color.green : Color.red;
                GUILayout.Label(importer.ActiveSplash ? "ACTIVE SPLASH" : "INACTIVE SPLASH", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft });
                GUI.color = defaultColor;      // Reset color back to default

                EditorGUILayout.Space(10);
                
                Color splashButtonColor = EditorGUIUtility.isProSkin ? Color.cyan : Color.blue;
                
                // Setting up for the "Activate Splash" button
                GUI.enabled = !importer.ActiveSplash;
                GUI.backgroundColor = importer.ActiveSplash ? Color.grey : splashButtonColor;
                if (GUILayout.Button("Activate Splash",GUILayout.Width(122), GUILayout.Height(32)))
                {
                    importer.ActiveSplash = true;
                    EditorUtility.SetDirty(importer);
                    ForceReimport(importer); // force save without need to hit Apply button
                    SplashdownController.SetSplash(importer.name); //set splash right-away 
                }

                // Restoring GUI state, then setting up for the "Deactivate" button
                GUI.backgroundColor = originalColor;
                GUI.enabled = importer.ActiveSplash;
                GUI.backgroundColor = importer.ActiveSplash ? splashButtonColor : Color.grey;
                if (GUILayout.Button("Deactivate Splash",GUILayout.Width(122), GUILayout.Height(32)))
                {
                    importer.ActiveSplash = false;
                    EditorUtility.SetDirty(importer);
                    ForceReimport(importer);
                    SplashdownController.RemoveSplash(importer.name);
                }
                GUI.backgroundColor = originalColor;
            }

            private void DrawIconActivationButtons(SplashdownImporter importer, Color originalColor)
            {
                Color defaultColor = GUI.color;
                GUI.color = importer.ActiveSplash ? Color.green : Color.red;
                GUILayout.Label(importer.ActiveIcon ? "ACTIVE ICON" : "INACTIVE ICON", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft});
                GUI.color = defaultColor;      // Reset color back to default

                
                EditorGUILayout.Space(10);
                
                Color IconButtonColor = EditorGUIUtility.isProSkin ? Color.cyan : Color.blue;

                // Setup for the "Activate Icon" button
                GUI.backgroundColor = originalColor;
                GUI.enabled = !importer.ActiveIcon;
                GUI.backgroundColor = importer.ActiveIcon ? Color.grey : IconButtonColor;
                if (GUILayout.Button("Activate Icon",GUILayout.Width(122), GUILayout.Height(32)))
                {
                    importer.ActiveIcon = true;
                    EditorUtility.SetDirty(importer);
                    ForceReimport(importer); // force save without need to hit Apply button
                }

                // Restoring GUI state, then setting up for the "Deactivate" button
                GUI.backgroundColor = originalColor;
                GUI.enabled = importer.ActiveIcon;
                GUI.backgroundColor = importer.ActiveIcon ? IconButtonColor : Color.grey;
                if (GUILayout.Button("Deactivate Icon",GUILayout.Width(122), GUILayout.Height(32)))
                {
                    importer.ActiveIcon = false;
                    EditorUtility.SetDirty(importer);
                    ForceReimport(importer);
                }

                // Restoring original GUI state
                GUI.backgroundColor = originalColor;
            }

    }
}