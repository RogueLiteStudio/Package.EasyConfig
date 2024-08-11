using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyConfig
{
    public class ConfigableEntityListView : IMGUIContainer, INotifyValueChanged<string>
    {
        private string searchKey;
        private string selectedPath;
        private Vector2 scrollPos;
        private List<ConfigableItem> searchResult = new List<ConfigableItem>();
        private bool disableScroll = false;
        private GUIStyle foldoutStyle;
        public ConfigableEntityListView()
        {
            onGUIHandler = OnGUI;
        }

        public string value
        {
            get => selectedPath;
            set
            {
                if (selectedPath == value)
                    return;
                string previousValue = selectedPath;
                selectedPath = value;
                if (disableScroll)
                    ScrollToItem(selectedPath);
                using (ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(previousValue, value))
                {
                    changeEvent.target = this;
                    SendEvent(changeEvent);
                }
            }
        }

        public void ScrollToItem(string path)
        {
            int line = 0;
            foreach (var item in ConfigableEntityCollector.instance.CatalogItems)
            {
                int index = item.Items.FindIndex(it => it.Path == path);
                if (index < 0)
                {
                    if (item.Foldout)
                    {
                        line += item.Items.Count;
                    }
                }
                else
                {
                    item.Foldout = true;
                    line += index;
                    break;
                }
            }
            scrollPos.y = line * EditorGUIUtility.singleLineHeight;
            MarkDirtyRepaint();
        }

        public void SetValueWithoutNotify(string newValue)
        {
            selectedPath = newValue;
            ScrollToItem(selectedPath);
        }

        private void DrawSearchField()
        {
            EditorGUI.BeginChangeCheck();
            searchKey = EditorGUILayout.TextField("", searchKey);
            if (EditorGUI.EndChangeCheck())
            {
                searchResult.Clear();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    foreach (var item in ConfigableEntityCollector.instance.CatalogItems)
                    {
                        foreach (var subItem in item.Items)
                        {
                            if (subItem.Name.Contains(searchKey, System.StringComparison.CurrentCultureIgnoreCase))
                            {
                                searchResult.Add(subItem);
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(searchKey) && GUILayout.Button("X", EditorStyles.boldLabel))
            {
                searchKey = "";
                searchResult.Clear();
                ScrollToItem(selectedPath);
            }
        }


        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                DrawSearchField();
            }
            using (var scroll = new GUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scroll.scrollPosition;
                if (searchResult.Count > 0)
                {
                    foreach (var item in searchResult)
                    {
                        DrawItem(item);
                    }
                }
                else
                {
                    if (foldoutStyle == null)
                    {
                        foldoutStyle = new GUIStyle(EditorStyles.foldout);
                        var offset = foldoutStyle.contentOffset;
                        offset.x += EditorGUIUtility.singleLineHeight;
                        foldoutStyle.contentOffset = offset;
                    }
                    foreach (var catalog in ConfigableEntityCollector.instance.CatalogItems)
                    {
                        catalog.Foldout = EditorGUILayout.Foldout(catalog.Foldout, catalog.Name, true, foldoutStyle);
                        if (catalog.Foldout)
                        {
                            foreach (var item in catalog.Items)
                            {
                                DrawItem(item);
                            }
                        }
                    }
                }
            }
        }

        private void DrawItem(ConfigableItem item)
        {
            bool isSelected = selectedPath == item.Path;
            if (isSelected)
            {
                Color color = GUI.backgroundColor;
                GUI.backgroundColor = Color.blue;
                EditorGUILayout.LabelField(item.Content, EditorStyles.boldLabel);
                GUI.backgroundColor = color;
            }
            else if(GUILayout.Button(item.Content, EditorStyles.label))
            {
                disableScroll = true;
                value = item.Path;
                disableScroll = false;
            }
        }
    }
}
