
using Splashdown.Editor;
using UnityEditor;

public class MyRules
{

    [MenuItem("Test/SplashTest")]
    public static void Test()
    {
        var options = new SplashdownOptions()
        {
            line1 = "asdads",
            line2 = "cruel"
        };
            
        //SplashdownGenerator.CreateNewSplashdown(options)
    }

}

