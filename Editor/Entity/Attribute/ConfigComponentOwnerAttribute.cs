using System;

namespace EasyConfig
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ConfigComponentOwnerAttribute : Attribute
    {
        public Type OwnerType { get; private set; }

        public ConfigComponentOwnerAttribute(Type ownerType)
        {
            OwnerType = ownerType;
        }
    }
}
