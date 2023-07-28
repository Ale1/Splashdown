using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Splashdown.Editor
{
    class SplashdownBuildProcessor : IPreprocessBuildWithReport
    {
        private static SplashdownImporter activeSplashdown = null;

        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (Application.isBatchMode)
                return;
            
            var guids = SplashdownController.FindAllSplashdownFiles();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                SplashdownImporter importer = (SplashdownImporter) AssetImporter.GetAtPath(path);
                
                if (importer.Activated)
                {
                    activeSplashdown = importer;
                    break;  //only use first match
                }
            }

            if (activeSplashdown != null)
            {
                SplashdownController.SetSplash(activeSplashdown.name);
                SplashdownController.SetIcons(activeSplashdown.name);
            }
            else
            {
                Debug.Log("nothing found");
            }
        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (Application.isBatchMode)
                return;
            
            if (activeSplashdown != null)
            {
                SplashdownController.RemoveSplash(activeSplashdown.name);
                SplashdownController.RestoreIcons();
                activeSplashdown = null;
            }
        
        }
    }
    
}