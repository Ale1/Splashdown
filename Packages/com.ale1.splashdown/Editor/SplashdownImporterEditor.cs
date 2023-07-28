
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Splashdown.Editor
{
    [CustomEditor(typeof(SplashdownImporter))]
    public class SplashdownImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            var importer = (SplashdownImporter)target;
            Options options = SplashdownImporter.DeserializeOptions(importer.assetPath);
            
            DrawDefaultInspector();
            
            // Draw the Options fields
            EditorGUI.BeginChangeCheck();
            options.line1 = EditorGUILayout.TextField("Line 1", options.line1);
            options.line2 = EditorGUILayout.TextField("Line 2", options.line2);
            options.line3 = EditorGUILayout.TextField("Line 3", options.line3);
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
            
            EditorGUILayout.Space(20);


            EditorGUILayout.BeginHorizontal();
            // Saving current GUI state
            bool prevState = GUI.enabled;
            var originalColor = GUI.backgroundColor;

            // Setting up for the "Activate" button
            GUI.enabled = !importer.Activated;
            GUI.backgroundColor = importer.Activated ? Color.grey : Color.green;
            if (GUILayout.Button("Activate",GUILayout.Width(98), GUILayout.Height(22)))
            {
                importer.Activated = true;
                EditorUtility.SetDirty(importer);
                SplashdownController.SetSplash(importer.name);
            }

            // Restoring GUI state, then setting up for the "Deactivate" button
            GUI.backgroundColor = originalColor;
            GUI.enabled = importer.Activated;
            GUI.backgroundColor = importer.Activated ? Color.red : Color.grey;
            if (GUILayout.Button("Deactivate",GUILayout.Width(98), GUILayout.Height(22)))
            {
                importer.Activated = false;
                EditorUtility.SetDirty(importer);
                SplashdownController.RemoveSplash(importer.name);
            }

            // Restoring original GUI state
            GUI.backgroundColor = originalColor;
            GUI.enabled = prevState;
            
            EditorGUILayout.EndHorizontal();
            

            ApplyRevertGUI();
        }
    }
}