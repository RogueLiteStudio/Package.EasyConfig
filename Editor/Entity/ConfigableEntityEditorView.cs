using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyConfig
{
    public class ConfigableEntityEditorView : VisualElement
    {
        private readonly VisualElement entityInfoView = new VisualElement();
        private readonly VisualElement componetsView = new VisualElement();
        private readonly IntegerField UIDField = new IntegerField("UID");
        private readonly TextField nameField = new TextField("显示名");
        private readonly TextField commentField = new TextField("备注");

        private readonly List<ConfigComponentView> componentViews = new List<ConfigComponentView>();

        protected ConfigableEntity entity;

        public ConfigableEntityEditorView()
        {
            InitEntityInfoView();
            Add(entityInfoView);
            Add(componetsView);
        }

        public void SetEntity(ConfigableEntity entity)
        {
            this.entity = entity;
            RefreshView();
        }

        public void OnUndoRedo()
        {
            RefreshView();
        }

        protected void RefreshView()
        {
            if (entity == null)
                return;
            RefreshEntityInfo();
            for (int i = 0; i < entity.Components.Count; ++i)
            {
                var data = entity.Components[i];
                ConfigComponentView view;
                if (i >= componentViews.Count)
                {
                    view = new ConfigComponentView();
                    componentViews.Add(view);
                    componetsView.Add(view);
                }
                else
                {
                    view = componentViews[i];
                }
                view.style.display = DisplayStyle.Flex;
                view.SetConfig(entity, data);
            }
            for (int i= entity.Components.Count; i< componentViews.Count; ++i)
            {
                var view = componentViews[i];
                view.style.display = DisplayStyle.None;
            }
        }

        protected virtual void InitEntityInfoView()
        {
            UIDField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("config UID modify");
                entity.UID = evt.newValue;
            });
            entityInfoView.Add(UIDField);
            nameField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("config name modify");
                entity.Name = evt.newValue;
            });
            entityInfoView.Add(nameField);
            commentField.RegisterValueChangedCallback(evt =>
            {
                RegisterUndo("config comment modify");
                entity.Comment = evt.newValue;
            });
            entityInfoView.Add(commentField);
        }

        protected virtual void RefreshEntityInfo()
        {
            UIDField.SetValueWithoutNotify(entity.UID);
            nameField.SetValueWithoutNotify(entity.name);
            commentField.SetValueWithoutNotify(entity.Comment);
        }
        protected void RegisterUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(entity, name);
            EditorUtility.SetDirty(entity);
        }
    }
}
