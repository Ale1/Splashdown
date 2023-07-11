using Splashdown;
using UnityEditor;
using UnityEditor.AssetImporters;

[CustomEditor(typeof(SplashdownImporter))]
public class SplashdownImporterEditor : ScriptedImporterEditor
{
    
    private SerializedObject serializedSplashdownImporter;

    private SerializedProperty includeInSplashProperty;
    private SerializedProperty includeInAppIconProperty;
    private SerializedProperty enableLoggingProperty;
    
    private SerializedProperty line1Property;
    private SerializedProperty line2Property;
    private SerializedProperty line3Property;
    
    
    public override void OnInspectorGUI()
    {
        // Cast the target to SplashdownImporter
        var importer = (SplashdownImporter)target;
        
        // If the serialized object is not initialized or if the target has changed
        if (serializedSplashdownImporter == null || serializedSplashdownImporter.targetObject != importer)
        {
            // Initialize the serialized object and get properties
            serializedSplashdownImporter = new SerializedObject(importer);
            
            includeInSplashProperty = serializedSplashdownImporter.FindProperty("includeInSplash");
            includeInAppIconProperty = serializedSplashdownImporter.FindProperty("includeInAppIcon");
            enableLoggingProperty = serializedSplashdownImporter.FindProperty("enableLogging");
            
            line1Property = serializedSplashdownImporter.FindProperty("line1");
            line2Property = serializedSplashdownImporter.FindProperty("line2");
            line3Property = serializedSplashdownImporter.FindProperty("line3");
        }

        // Update the serialized object
        serializedSplashdownImporter.Update();

        // Draw the properties using EditorGUILayout.PropertyField
        EditorGUILayout.PropertyField(includeInSplashProperty);
        EditorGUILayout.PropertyField(includeInAppIconProperty);
        EditorGUILayout.PropertyField(enableLoggingProperty);
        EditorGUILayout.Space(20);
        EditorGUILayout.PropertyField(line1Property);
        EditorGUILayout.PropertyField(line2Property);
        EditorGUILayout.PropertyField(line3Property);

        // Apply changes to the serialized object only if modifications were made
        if (serializedSplashdownImporter.hasModifiedProperties)
        {
            serializedSplashdownImporter.ApplyModifiedProperties();
        }

        // Add ApplyRevertGUI
        base.ApplyRevertGUI();
    }
}