
using Splashdown.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Splashdown.Editor
{


    [CustomEditor(typeof(SplashdownImporter))]
    public class SplashdownImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Add ApplyRevertGUI
            base.ApplyRevertGUI();
        }
    }
}