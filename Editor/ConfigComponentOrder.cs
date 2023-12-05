using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyConfig
{
    public static class ConfigComponentOrder
    {
        private static Dictionary<Type, int> orderMap = new Dictionary<Type, int>();

        public static int GetOrder(Type type)
        {
            if (orderMap.TryGetValue(type, out var order))
            {
                return order;
            }
            var attr = type.GetCustomAttribute<ConfigSortOrderAttritube>(false);
            order = attr?.Order ?? ConfigSortOrderAttritube.DefaultOrder;
            orderMap.Add(type, order);
            return order;
        }
    }
}
