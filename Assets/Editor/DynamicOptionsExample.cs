using UnityEngine;

public static class Example  
{
        
        [Splashdown.OptionsProvider]
        public static Splashdown.Editor.Options ProvideSplashdownOptions() => new()
        {
                line1 = "Hello",
                line2 = "Cruel",
                line3 = "World",
                textColor = Color.red
        };
        
        
}



