using UnityEditor;

namespace Splashdown.CI
{
    public class BuildTools
    {
        // Start is called before the first frame update
        [MenuItem("Splashdown/build")]
        public static void AutoBuild()
        {
            string buildPath = "Builds/iOS";
            
            BuildPipeline.BuildPlayer(new[] { "Assets/Test/SampleScene.unity" }, buildPath, BuildTarget.iOS, BuildOptions.StrictMode);
        }

    }
}

