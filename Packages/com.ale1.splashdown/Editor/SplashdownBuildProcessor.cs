using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Splashdown.Editor
{
    class SplashdownBuildProcessor : IPreprocessBuildWithReport
    {
        private static List<SplashdownImporter> activeSplashes = new List<SplashdownImporter>();
        private static SplashdownImporter activeIcon = null;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var guids = SplashdownController.FindAllSplashdownFiles();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                SplashdownImporter importer = (SplashdownImporter) AssetImporter.GetAtPath(path);
                
                if (importer.ActiveSplash)
                {
                    activeSplashes.Add(importer);
                }
            }

            if (activeSplashes.Count > 0)
            {
                foreach (var activeSplash in activeSplashes)
                {
                    SplashdownController.SetSplash(activeSplash.name);
                }
                
            }
            else
            {
                Debug.Log("Splashdown :: BuildProcessor :: nothing found");
            }
            
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                SplashdownImporter importer = (SplashdownImporter) AssetImporter.GetAtPath(path);
                
                if (importer.ActiveIcon)
                {
                    activeIcon = importer;
                    break;  //we only want first match
                }
            }

        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            foreach (var activeSplash in activeSplashes)
            {
                SplashdownController.RemoveSplash(activeSplash.name);
            }
            activeSplashes = null;
            
            if(activeIcon != null)
                SplashdownController.RestoreIcons();

            activeIcon = null;
    
        }
    }
    
}