using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyConfig
{
    public class ConfigComponentView : InspectorElement
    {
        private readonly TextField commendField = new TextField("描述");
        private readonly IMGUIContainer editorView = new IMGUIContainer();
        private ConfigComponentInpsector inpsector;

        public ConfigComponentView()
        {
            OnContextMenue = ContextMenue;
            commendField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("config commend modify");
                inpsector.Config.Comment = evt.newValue;
            });
            Add(commendField);
            editorView.onGUIHandler = OnHandleGUI;
            Add(editorView);
        }

        public void SetConfig(ConfigableEntity entity, ConfigComponentData data)
        {
            if (inpsector!=null && entity == inpsector.Entity && inpsector.Config == data)
            {
                editorView.MarkDirtyRepaint();
                return;
            }
            System.Type type = data.Data.GetType();
            var dpName = type.GetCustomAttribute<DisplayNameAttribute>();
            if (dpName != null)
            {
                Text = dpName.Name;
                tooltip =  dpName.ToolTip;
            }
            else
            {
                Text = type.Name;
            }
            inpsector = ConfigComponentInpsector.CreateInspector(entity, data);
            editorView.MarkDirtyRepaint();
        }

        private void OnHandleGUI()
        {
            inpsector?.OnInsperctorGUI();
        }

        private void ContextMenue(GenericMenu menu)
        {
            if (inpsector == null)
                return;
            menu.AddItem(new GUIContent("复制"), false, () => ConfigableEntityClipboard.instance.CopyConfigData(inpsector.Config));
            if (ConfigableEntityClipboard.instance.CheckPaste(inpsector.Config))
            {
                menu.AddItem(new GUIContent("粘贴"), false, () =>
                {
                    RegisterUndo("config paste");
                    ConfigableEntityClipboard.instance.PasteConfigData(inpsector.Config, false);
                });
                menu.AddItem(new GUIContent("粘贴(仅属性)"), false, () =>
                {
                    RegisterUndo("config paste");
                    ConfigableEntityClipboard.instance.PasteConfigData(inpsector.Config, true);
                });
            }
            inpsector.OnContextMenu(menu);
        }

        protected void RegisterUndo(string name)
        {
            if (inpsector == null)
                return;
            Undo.RegisterCompleteObjectUndo(inpsector.Entity, name);
            EditorUtility.SetDirty(inpsector.Entity);
        }
    }
}
