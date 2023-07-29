using Splashdown.Editor;
using UnityEditor;

public static class Debug
{
    public static string Testfile = "MySplashdown";
    private static Options options => SplashdownUtils.GetOptionsFromSplashdownName(Testfile);
    private static string guid => SplashdownUtils.GetGuidBySplashdownName(Testfile); 
    
    [MenuItem("Splashdown/test set splash")]
    public static void TestSetSplash()
    {
        SplashdownController.SetSplash(guid);
    }

    [MenuItem("Splashdown/test set icons")]
    public static void TestSetIcons()
    {
        SplashdownController.SetIcons(guid);
    }

    [MenuItem("Splashdown/test restore splash")]
    public static void TestRestoreSplash()
    {
        SplashdownController.RemoveSplash(guid);
    }

    [MenuItem("Splashdown/test restore icons")]
    public static void TestRestoreIcons()
    {
        SplashdownController.RestoreIcons();
    }
}
