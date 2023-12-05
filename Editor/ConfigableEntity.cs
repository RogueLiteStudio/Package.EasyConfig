using System.Collections.Generic;
using UnityEngine;
namespace EasyConfig
{
    public class ConfigableEntity : ScriptableObject
    {
        public int UID;//全局唯一ID，运行时用
        public string Name;//显示用名字
        [SerializeField]
        private List<ConfigComponentData> components = new List<ConfigComponentData>();
        public IReadOnlyList<ConfigComponentData> Components=> components;

        public T AddComponent<T>() where T : class, IConfigComponent, new()
        {
            foreach (var comp in components)
            {
                if (comp.Data is T data)
                {
                    return data;
                }
            }
            var newData = new T();
            var newComp = new ConfigComponentData();
            newComp.SetData(newData);
            components.Add(newComp);
            return newData;
        }

        public T GetComponent<T>() where T : class, IConfigComponent
        {
            foreach (var comp in components)
            {
                if (comp.Data is T data)
                {
                    return data;
                }
            }
            return null;
        }

        public void RemoveComponent<T>() where T : IConfigComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].Data is T)
                {
                    components.RemoveAt(i);
                    return;
                }
            }
        }
    }
}