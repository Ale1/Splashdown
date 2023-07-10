using UnityEditor;
using UnityEngine;
using Splashdown;

[InitializeOnLoad]
public static class Example 
{
    static Example()
    {
        Config.replaceIcon = true;
        Config.replaceSplash = true;
        Config.logging = true;
        
        Config.line1 = "jiDgoT";
        Config.line2 = "12/10/98";
        Config.line3 = Application.unityVersion;
        
    }
}
