using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Splashdown.Editor
{
    class SplashdownBuildProcessor : IPreprocessBuildWithReport
    {
        private static List<String> _activeSplashes = new List<String>();
        private static String _activeIcon = null;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // We put this in a try-catch block because the functionality provided by Splashdown is not critical.
            // The build is still viable even with Splashdown failing. 
            try
            {
                var guids = SplashdownUtils.FindAllSplashdownGUIDs();

                var importers = new List<SplashdownImporter>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    SplashdownImporter importer = (SplashdownImporter) AssetImporter.GetAtPath(path);
                    if(importer != null)
                        importers.Add(importer);
                }

                _activeSplashes = importers
                    .Where(x => x.IsSplashActive)
                    .Select(x=> x.GetGuid)
                    .ToList();
                
                _activeSplashes.ForEach(SplashdownController.SetSplash);
               
                _activeIcon = importers
                    .Where(x => x.IsIconActive)
                    .Select(x=> x.GetGuid)
                    .FirstOrDefault();

                //we only process the first result as we cant have multiole icons.
                if (_activeIcon != null)
                {
                    SplashdownController.SetIcons(_activeIcon);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            
           
        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            _activeSplashes.ForEach(SplashdownController.RemoveSplash);
            _activeSplashes = null;
            
            if(_activeIcon != null)
                SplashdownController.RestoreIcons();

            _activeIcon = null;
    
        }
    }
    
}