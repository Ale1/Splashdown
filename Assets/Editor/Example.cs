using UnityEngine;

public static class Example  
{
        [Splashdown.OptionsProvider]
        public static Splashdown.Options ProvideSplashdownOptions()
        {
            var options = new Splashdown.Options()
            {
                line1 = "Hello",
                line2 = "Cruel",
                line3 = "World",
                textColor = Color.red
            };

            Debug.Log("Applying dynamic splashdown options");
            
            return options;
        }
}

