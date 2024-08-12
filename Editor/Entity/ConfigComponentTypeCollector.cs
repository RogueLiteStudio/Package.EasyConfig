using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyConfig
{
    public struct ConfigComponentTypeData
    {
        public string DisplyName;
        public string ToolTip;
        public Type Type;
    }
    public class ConfigComponentTypeCollector
    {
        private static ConfigComponentTypeCollector instance;
        private readonly Dictionary<Type, List<ConfigComponentTypeData>> componentTypeMap = new Dictionary<Type, List<ConfigComponentTypeData>>();

        public static ConfigComponentTypeCollector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigComponentTypeCollector();
                    instance.Collector();
                }
                return instance;
            }
        }

        public static IReadOnlyList<ConfigComponentTypeData> GetConfigComponentTypes(Type ownerType)
        {
            if (Instance.componentTypeMap.TryGetValue(ownerType, out var list))
            {
                return list;
            }
            return null;
        }

        private void Collector()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("Unity")
                    || assembly.FullName.StartsWith("com.unity")
                    || assembly.FullName.StartsWith("System")
                    || assembly.FullName.StartsWith("mscorlib"))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || !typeof(IConfigComponent).IsAssignableFrom(type))
                        continue;
                    var ownerAttributes = type.GetCustomAttributes<ConfigComponentOwnerAttribute>(true);
                    foreach (var attribute in ownerAttributes)
                    {
                        if (!componentTypeMap.TryGetValue(attribute.OwnerType, out var list))
                        {
                            list = new List<ConfigComponentTypeData>();
                            componentTypeMap.Add(attribute.OwnerType, list);
                        }
                        else if(list.Exists(it=>it.Type == type))
                        {
                            continue;
                        }
                        list.Add(ToTypeData(type));
                    }
                }
            }
            HashSet<Type> handlerTypes = new HashSet<Type>();
            foreach (var kv in componentTypeMap)
            {
                UpdateBaseType(kv.Key, handlerTypes);
            }
        }

        private void UpdateBaseType(Type type, HashSet<Type> handlerTypes)
        {
            if (handlerTypes.Contains(type))
                return;
            handlerTypes.Add(type);
            var baseType = type.BaseType;
            if (baseType != null && componentTypeMap.ContainsKey(baseType))
            {
                UpdateBaseType(baseType, handlerTypes);
            }
            if (componentTypeMap.TryGetValue(type, out var list))
            {
                foreach (var t in list)
                {
                    if (componentTypeMap.TryGetValue(baseType, out var parentList))
                    {
                        if (parentList.Exists(it => it.Type == t.Type))
                            continue;
                        parentList.Add(t);
                    }
                }
            }
        }

        private ConfigComponentTypeData ToTypeData(Type type)
        {
            var attributes = type.GetCustomAttribute<DisplayNameAttribute>(false);
            if (attributes != null) 
            {
                return new ConfigComponentTypeData
                {
                    DisplyName = attributes.Name,
                    ToolTip = attributes.ToolTip,
                    Type = type
                };
            }
            return new ConfigComponentTypeData
            {
                DisplyName = type.Name,
                Type = type
            };
        }

    }
}
