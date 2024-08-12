using System;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public class ConfigSortOrderAttritube : Attribute
{
    public const int DefaultOrder = 0;
    public int Order { get; private set; }
    public ConfigSortOrderAttritube(int order)
    {
        Order = order;
    }
}
