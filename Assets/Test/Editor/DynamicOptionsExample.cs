using UnityEngine;

public static class Example  
{ 
        [Splashdown.OptionsProvider("MySplashdown")]
        public static Splashdown.Editor.Options ProvideSplashdownOptions() => new()
        {
                line1 = "Hello",
                line2 = "Handsome",
                textColor = Color.red
        };
       
        
        /* WIP
        [Splashdown.ActivationProvider]
        public static bool ProvideSplashdownActivationOverride(bool current)
        {
                if (UnityEngine.Debug.isDebugBuild)
                {
                        return true;
                }
                else
                {
                        return current;
                }
        }
        */
}


