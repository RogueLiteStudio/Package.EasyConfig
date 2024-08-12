using System;

namespace EasyConfig
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigableEntityEditorAttribute : Attribute
    {
        public Type Type { get; private set; }
        public ConfigableEntityEditorAttribute(Type type)
        {
            Type = type;
        }
    }
}
