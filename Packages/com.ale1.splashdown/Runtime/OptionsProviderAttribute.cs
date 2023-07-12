using System;


namespace Splashdown
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsProviderAttribute : Attribute
    {
        public OptionsProviderAttribute()
        {
        }
    }
}