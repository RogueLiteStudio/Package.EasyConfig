using System;

namespace EasyConfig
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ConfigableEntityCatalogAttribute : Attribute
    {
        public string CatalogName { get; private set; }
        public ConfigableEntityCatalogAttribute(string catalogName)
        {
            CatalogName = catalogName;
        }
    }
}
