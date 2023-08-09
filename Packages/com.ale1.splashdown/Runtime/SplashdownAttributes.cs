using System;


namespace Splashdown
{

    public abstract class SplashdownAttribute : Attribute
    {
        public string Filter { get; protected set;}
    }
    
    
    
    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsProviderAttribute : SplashdownAttribute
    {
        public OptionsProviderAttribute() 
        {
            Filter = null; 
        }
        
        public OptionsProviderAttribute(string filter)
        {
            Filter = filter;
        }
        
    }

    public class ActivationProviderAttribute : SplashdownAttribute
    {
        public ActivationProviderAttribute()
        {
            Filter = null;
        }

        public ActivationProviderAttribute(string filter)
        {
            Filter = filter;
        }
    }
}