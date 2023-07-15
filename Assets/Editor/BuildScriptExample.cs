using System.Collections;
using System.Collections.Generic;
using Splashdown.Editor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

class SplashdownBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        SplashdownController.SetSplash("MyCustomSplash");
    }
}