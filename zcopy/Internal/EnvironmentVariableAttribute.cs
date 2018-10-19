using System;

namespace BananaHomie.ZCopy.Internal
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class EnvironmentVariableAttribute : Attribute
    {
        public EnvironmentVariableAttribute(string name, object defaultValue = null)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public string Name { get; set; }
        public object DefaultValue { get; set; }
    }
}