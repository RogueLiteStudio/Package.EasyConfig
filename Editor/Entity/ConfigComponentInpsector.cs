using PropertyEditor;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EasyConfig
{
    public class ConfigComponentInpsector
    {
        public ConfigableEntity Entity;
        public ConfigComponentData Config;
        protected IDrawer drawer;
        private bool foldout = true;
        private string typeName;
        private string tooltip;
        public static GUIStyle backGrountStyle = new GUIStyle("OL box NoExpand") { padding = new RectOffset(1, 0, 0, 0) };
        public static GUIStyle toolbar = new GUIStyle("IN Title") { padding = new RectOffset(0, 0, 2, 0) };

        protected virtual void OnCreate()
        {
            System.Type type = Config.Data.GetType();
            drawer = DrawerCollector.CreateDrawer(type);
            var dpName = type.GetCustomAttribute<DisplayNameAttribute>();
            typeName = dpName != null ? dpName.Name : type.Name;
            if (dpName != null)
                tooltip = dpName.ToolTip;
        }
        public virtual void OnInsperctorGUI()
        {
            drawer.Draw(null, Config.Data, Entity);
        }
        protected void RegisterUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(Entity, name);
            EditorUtility.SetDirty(Entity);
        }
        public virtual void OnContextMenu(GenericMenu menu)
        {
        }
        protected bool LayoutGUI<T>(string label, ref T v, System.Func<string, T, GUILayoutOption[], T> func, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            var newVal = func(label, v, options);
            if (EditorGUI.EndChangeCheck())
            {
                RegisterUndo("property modify");
                v = newVal;
                return true;
            }
            return false;
        }
        private static Dictionary<System.Type, System.Type> inspectorTypes;
        internal static ConfigComponentInpsector CreateInspector(ConfigableEntity entity, ConfigComponentData config)
        {
            if (inspectorTypes == null)
            {
                inspectorTypes = new Dictionary<System.Type, System.Type>();
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.IsSubclassOf(typeof(ConfigComponentInpsector)))
                    {
                        var attr = type.GetCustomAttribute<ConfigableEntityEditorAttribute>();
                        if (attr != null)
                        {
                            inspectorTypes[attr.Type] = type;
                        }
                    }
                }
            }
            if (inspectorTypes.TryGetValue(config.Data.GetType(), out System.Type inspectorType))
            {
                var inspectot = (ConfigComponentInpsector)System.Activator.CreateInstance(inspectorType);
                inspectot.Entity = entity;
                inspectot.Config = config;
                inspectot.OnCreate();
            }
            var defaultInspector = new ConfigComponentInpsector { Entity = entity, Config = config };
            defaultInspector.OnCreate();
            return defaultInspector;
        }
    }

    public class TConfigableEntityInpsector<T> : ConfigComponentInpsector where T : class, IConfigComponent
    {
        protected T Data => Config.Data as T;
    }
}
