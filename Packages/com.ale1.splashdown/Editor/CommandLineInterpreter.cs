using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Splashdown.Editor;
using UnityEngine;
using UnityEditor;

namespace Splashdown.Editor
{
    public class CommandLineInterpreter
    {
        //e.g // /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/UnityProjects/MyProject -executeMethod MyEditorScript.PerformSetSplashWithOptions -name MySplashdown -l1 hello -l2 cruel -l3 world
        public CommandLineInterpreter()
        {
            
        }

        [MenuItem("Splashdown/test")]
        public static void SetSplashOptions()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string name = null;
            string l1 = null;
            string l2 = null;
            string l3 = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-name") name = args[i + 1];
                else if (args[i] == "-l1") l1 = args[i + 1];
                else if (args[i] == "-l2") l2 = args[i + 1];
                else if (args[i] == "-l3") l3 = args[i + 1];
            }

            name = "MySplashdown"; //todo: remove
            
            if (name == null)
            {
                //todo: if no name is passed, just use the first found.
                Debug.LogError("name cannot be empty");
                return;
            }
                

            Options newOpts = new(false)
            {
                line1 = "handsome", //l1, //todo: remove
                line2 = "banana", //l2, //todo: remove
                line3 = l3,
            };

            var guid = SplashdownController.FindSplashdownByName(name);
            
            var splashdownData = SplashdownController.LoadOptionsFromAssetDatabase(guid);
            
            if (splashdownData == null)
            {
                Debug.LogError($"no splashdown file found with name {name}");
                return;
            }

            splashdownData.UpdateWith(newOpts);
            
            var key = "com.ale1.Splashdown." + name;  //todo: move to constants
            var value = JsonUtility.ToJson(splashdownData);
            Debug.Log("key:" + "com.ale1.Splashdown."+name);
            EditorPrefs.SetString(key, value);
            
         
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
