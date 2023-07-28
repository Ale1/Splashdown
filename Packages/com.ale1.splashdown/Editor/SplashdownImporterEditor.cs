
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
            
            #region Label
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status:", GUILayout.MaxWidth(50));
            Color defaultColor = GUI.color;

            if (importer.Activated) 
            {
                GUI.color = Color.green;
            } 
            else 
            {
                GUI.color = Color.red;
            }

            EditorGUILayout.LabelField(importer.Activated ? "ACTIVE" : "INACTIVE");

            // Reset color back to default
            GUI.color = defaultColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            #endregion Label
            
            #region Activation Buttons
            EditorGUILayout.BeginHorizontal();
            // Saving current GUI state
            bool prevState = GUI.enabled;
            var originalColor = GUI.backgroundColor;

            Color buttonColor = EditorGUIUtility.isProSkin ? Color.cyan : Color.blue;

            // Setting up for the "Activate" button
            GUI.enabled = !importer.Activated;
            GUI.backgroundColor = importer.Activated ? Color.grey : buttonColor;
            if (GUILayout.Button("Activate",GUILayout.Width(108), GUILayout.Height(32)))
            {
                importer.Activated = true;
                EditorUtility.SetDirty(importer);
                ForceReimport(importer); // force save without need to hit Apply button
                SplashdownController.SetSplash(importer.name);
            }

            // Restoring GUI state, then setting up for the "Deactivate" button
            GUI.backgroundColor = originalColor;
            GUI.enabled = importer.Activated;
            GUI.backgroundColor = importer.Activated ? buttonColor : Color.grey;
            if (GUILayout.Button("Deactivate",GUILayout.Width(108), GUILayout.Height(32)))
            {
                importer.Activated = false;
                EditorUtility.SetDirty(importer);
                ForceReimport(importer);
                SplashdownController.RemoveSplash(importer.name);
            }

            // Restoring original GUI state
            GUI.backgroundColor = originalColor;
            GUI.enabled = prevState;
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
            #endregion Activation Buttons
            
            
            DrawDefaultInspector();
            
            //during reimport options can be null for a frame before inspector fetches new options, breaking inspector. 
            if (options == null)
                return;
            
            // Draw the Options fields
            EditorGUI.BeginChangeCheck();
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
                EditorGUILayout.LabelField("Using Font:", "Splashdown Default (Roboto Mono)");
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
    }
}