using PropertyEditor;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EasyConfig
{
    public class ConfigableEntityInpsector
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
        public void OnInsperctorGUI()
        {
            using (new GUILayout.VerticalScope(backGrountStyle))
            {
                using (new GUILayout.HorizontalScope(toolbar))
                {
                    EditorGUI.BeginChangeCheck();
                    bool enable = EditorGUILayout.Toggle(Config.Enabled, GUILayout.Width(20));
                    if (EditorGUI.EndChangeCheck())
                    {
                        RegisterUndo("config enable modify");
                        Config.Enabled = enable;
                    }
                    if (GUILayout.Button("", "PaneOptions"))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("复制"), false, () => ConfigableEntityClipboard.instance.CopyConfigData(Config));
                        if (ConfigableEntityClipboard.instance.CheckPaste(Config))
                        {
                            menu.AddItem(new GUIContent("粘贴"), false, () =>
                            {
                                RegisterUndo("config paste");
                                ConfigableEntityClipboard.instance.PasteConfigData(Config, false);
                            });
                            menu.AddItem(new GUIContent("粘贴(仅属性)"), false, () =>
                            {
                                RegisterUndo("config paste");
                                ConfigableEntityClipboard.instance.PasteConfigData(Config, true);
                            });
                        }
                        OnContextMenu(menu);
                        menu.ShowAsContext();
                    }
                    GUILayout.Space(5);
                    foldout = EditorGUILayout.Foldout(foldout, new GUIContent($"{typeName} - {Config.Name}", tooltip), true);
                    if (!foldout)
                        return;
                    LayoutGUI("名字", ref Config.Name, EditorGUILayout.TextField, GUILayout.Width(100));
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("描述");
                        EditorGUI.BeginChangeCheck();
                        string commit = EditorGUILayout.TextArea(Config.Comment);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("config comment modify");
                            Config.Comment = commit;
                        }
                    }

                }
            }
        }
        protected void RegisterUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(Entity, name);
            EditorUtility.SetDirty(Entity);
        }
        protected virtual void OnContextMenu(GenericMenu menu)
        {
        }
        protected virtual void DrawConfigInspector()
        {
            drawer.Draw(null, Config.Data, Entity);
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
        internal static ConfigableEntityInpsector CreateInspector(ConfigableEntity entity, ConfigComponentData config)
        {
            if (inspectorTypes == null)
            {
                inspectorTypes = new Dictionary<System.Type, System.Type>();
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.IsSubclassOf(typeof(ConfigableEntityInpsector)))
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
                var inspectot = (ConfigableEntityInpsector)System.Activator.CreateInstance(inspectorType);
                inspectot.Entity = entity;
                inspectot.Config = config;
                inspectot.OnCreate();
            }
            var defaultInspector = new ConfigableEntityInpsector { Entity = entity, Config = config };
            defaultInspector.OnCreate();
            return defaultInspector;
        }
    }

    public class TConfigableEntityInpsector<T> : ConfigableEntityInpsector where T : class, IConfigComponent
    {
        protected T Data => Config.Data as T;
    }
}
