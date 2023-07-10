using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using Splashdown;


    class PreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return Config.PreBuildCallbackOrder; }
        } 

        public void OnPreprocessBuild(BuildReport report)
        {
            if (Config.enabled)
                SplashdownController.Build();
        }
    }

    public class PostBuildprocessor
    {
        [PostProcessBuild(Config.PostbuildCallbackOrder)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (Config.enabled)
                SplashdownController.Restore();
        }
    }
