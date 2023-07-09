using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;


namespace Splashdown
{

    class PreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return Config.PreBuildCallbackOrder; }
        } 

        public void OnPreprocessBuild(BuildReport report)
        {
            if (Config.enabled)
                SplashdownController.Run();
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
}