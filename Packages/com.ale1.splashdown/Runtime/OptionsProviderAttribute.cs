using System;


namespace Splashdown
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsProviderAttribute : Attribute
    {
        public OptionsProviderAttribute() 
        {
            Filter = null; 
        }
        
        public OptionsProviderAttribute(string filter)
        {
            Filter = filter;
        }

        public string Filter { get; }
    }
}