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
                var arg = args[i].ToLower();
                
                if (arg == "-name") name = args[i + 1];
                else if (arg == "-enable_splash") useSplash = true;
                else if (arg == "-enable_icon") useIcon = true;
                else if (arg == "-disable_splash") useSplash = false;
                else if (arg == "-disable_icon") useIcon = false;
                else if (arg == "-disable_dynamic") useDynamic = false;
                else if (arg == "-enable_dynamic") useDynamic = true;
                else if (arg == "-l1") l1 = args[i + 1];
                else if (arg == "-l2") l2 = args[i + 1];
                else if (arg == "-l3") l3 = args[i + 1];
                
                //todo: add font size as CLI-compatible option
                //else if (arg == "-size") fontSize = args[i + 1];
            }
            
            if (String.IsNullOrEmpty(name))
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

            var guid = SplashdownUtils.GetGuidBySplashdownName(name);
            var opts = SplashdownUtils.GetOptionsFromGuid(guid);
            
            if (opts == null)
            {
                Debug.LogError($"Splashdown :: no splashdown file found with name {name}");
                return;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(assetPath) as SplashdownImporter;
            if (importer == null)
            {
                Debug.LogError("Splashdown :: no importer found ");
                return;
            }

            if (useDynamic != null && useDynamic != importer.useDynamicOptions)
            {
                importer.useDynamicOptions = (bool) useDynamic;
            }

            if (useSplash != null && importer.IsSplashActive != useSplash)
            {
                importer.SetActiveSplash( (bool) useSplash);
            }

            if (importer != null && importer.IsIconActive != useIcon)
            {
                importer.SetActiveIconWithEvent( (bool) useIcon);
            }
            
            opts.UpdateWith(newOpts);
            importer.SaveAndReimport();
            
            var key =  Constants.EditorPrefsKey + "." + name;
            SplashdownUtils.SaveOptionsToEditorPrefs(key, opts);
                
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
