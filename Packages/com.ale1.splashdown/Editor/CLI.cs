using System;
using UnityEngine;
using UnityEditor;

namespace Splashdown.Editor
{
    public class CLI
    {
        //Example: /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/Desktop/Splashdown -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -activeSplash -l1 hello -l2 cruel -l3 world
        
        public CLI()
        {
            
        }

        [MenuItem("Splashdown/test")]
        public static void SetSplashOptions()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string name = null;
            bool activeSplash = false;
            bool activeIcon = false;
            bool? useDynamic = null;
            string l1 = null;
            string l2 = null;
            string l3 = null;

            for (int i = 0; i < args.Length; i++)
            {

                //todo: ignore caps
                if (args[i] == "-name") name = args[i + 1];
                else if (args[i].ToLower() == "-active_splash") activeSplash = true;
                else if (args[i].ToLower() == "-active_icon") activeIcon = true;
                else if (args[i].ToLower() == "-disable_dynamic") useDynamic = false;
                else if (args[i].ToLower() == "-enable_dynamic") useDynamic = true;
                else if (args[i].ToLower() == "-l1") l1 = args[i + 1];
                else if (args[i].ToLower() == "-l2") l2 = args[i + 1];
                else if (args[i].ToLower() == "-l3") l3 = args[i + 1];
            }
            
            if (name == null)
            {
                Debug.LogError("name cannot be empty");
                return;
            }
            
            Options newOpts = new(false)
            {
                line1 = l1, 
                line2 = l2,
                line3 = l3,
            };

            var guid = SplashdownController.FindSplashdownByName(name);
            
            var splashdownData = SplashdownController.LoadOptionsFromSplashdownFile(guid);
            
            if (splashdownData == null)
            {
                Debug.LogError($"Splashdown :: no splashdown file found with name {name}");
                return;
            }
            
            var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as SplashdownImporter;
            if (importer == null)
            {
                Debug.LogError("Splashdown :: no importer found ");
                Debug.LogError("guid" + guid);
                Debug.LogError("path:" +AssetDatabase.GUIDToAssetPath(guid));
            }
            
            if (importer.Activated != activeSplash)
            {
                importer.Activated = activeSplash;
                importer.useDynamicOptions = useDynamic ?? importer.useDynamicOptions;
                importer.SaveAndReimport();
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
