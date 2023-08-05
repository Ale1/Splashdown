using System;
using UnityEngine;
using UnityEditor;

namespace Splashdown.Editor
{
    public static class CLI
    {
        /// <summary>
        /// Parses command line arguments to set various options for Splashdown.
        /// </summary>
        /// <remarks>
        /// The method takes command line arguments to find a splashdown file by name (required), 
        /// then toggle its use as a splash (optional),
        /// toggle use as icon (optional), 
        /// toggle dynamic behavior (optional), 
        /// set three lines of text (optional),
        /// and define the font size (optional). 
        /// If the provided name is empty, it logs an error and returns.
        /// The options are updated using the provided arguments, and the asset is then reimported.
        ///
        /// This method is intended to be called exclusively from an external command line. It is not
        /// intended to be called programmatically from within the application.
        /// </remarks>
        /// <example>
        /// An example of using this method with a command line instruction:
        /// <code>
        /// /Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ~/Desktop/Splashdown -executeMethod Splashdown.Editor.CLI.SetSplashOptions -name MySplashdown -activeSplash -l1 hello -l2 cruel -l3 world -size 23
        /// </code>
        /// The `-name` parameter is required. Other parameters such as `-activeSplash`, `-l1`, `-l2`, `-l3`, and `-size` are optional.
        /// </example>
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
            int? size = null;

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
                else if (arg == "-size") size = Int32.TryParse(args[i + 1], out int inputSize) ? inputSize : null;
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
                TargetFontSize = (SerializableInt) size
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

            if (useIcon != null && importer.IsIconActive != useIcon)
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
