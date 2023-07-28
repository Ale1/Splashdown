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
            bool? useSplash = null;
            bool? useIcon = null;
            bool? useDynamic = null;
            string l1 = null;
            string l2 = null;
            string l3 = null;

            for (int i = 0; i < args.Length; i++)
            {

                //todo: ignore caps
                if (args[i] == "-name") name = args[i + 1];
                else if (args[i].ToLower() == "-enable_splash") useSplash = true;
                else if (args[i].ToLower() == "-enable_icon") useIcon = true;
                else if (args[i].ToLower() == "-disable_splash") useSplash = false;
                else if (args[i].ToLower() == "-disable_icon") useIcon = false;
                else if (args[i].ToLower() == "-disable_dynamic") useDynamic = false;
                else if (args[i].ToLower() == "-enable_dynamic") useDynamic = true;
                else if (args[i].ToLower() == "-l1") l1 = args[i + 1];
                else if (args[i].ToLower() == "-l2") l2 = args[i + 1];
                else if (args[i].ToLower() == "-l3") l3 = args[i + 1];
            }
            
            if (name == null)
            {
                Debug.LogError("Splashdown :: name cannot be empty");
                return;
            }
            
            Options newOpts = new(false)
            {
                line1 = l1, 
                line2 = l2,
                line3 = l3,
            };

            var guid = SplashdownController.FindSplashdownByName(name);
            
            var splashdownData = SplashdownController.GetOptionsFromGUID(guid);
            
            if (splashdownData == null)
            {
                Debug.LogError($"Splashdown :: no splashdown file found with name {name}");
                return;
            }
            
            var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as SplashdownImporter;
            if (importer == null)
            {
                Debug.LogError("Splashdown :: no importer found ");
                return;
            }

            if (useDynamic != null && useDynamic != importer.useDynamicOptions)
            {
                importer.useDynamicOptions = (bool) useDynamic;
            }

            if (useSplash != null && importer.ActiveSplash != useSplash)
            {
                importer.ActiveSplash = (bool) useSplash;
            }

            if (importer != null && importer.ActiveIcon != useIcon)
            {
                importer.ActiveIcon = (bool) useIcon;
            }
            
            splashdownData.UpdateWith(newOpts);
            importer.SaveAndReimport();
            
            var key =  Constants.EditorPrefsKey + "." + name; 
            var value = JsonUtility.ToJson(splashdownData);
            EditorPrefs.SetString(key, value);
            
         
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
