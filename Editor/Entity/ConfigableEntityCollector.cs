using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EasyConfig
{
    [System.Serializable]
    public class ConfigableItem
    {
        public int CatalogIndex;
        public int IndexInCatalog;
        public string Path;
        public string Name;
        public GUIContent Content;
    }
    [System.Serializable]
    public class ConfigableCatalogItem
    {
        public bool Foldout;
        public string Name;
        public Texture2D Icon;
        public List<ConfigableItem> Items = new List<ConfigableItem>();
    }
    public class ConfigableEntityCollector : ScriptableSingleton<ConfigableEntityCollector>
    {
        internal static bool hasInstance = false;
        [SerializeField]
        internal List<string> entityPaths = new List<string>();
        [SerializeField]
        private List<ConfigableCatalogItem> catalogItems;
        public IReadOnlyList<ConfigableCatalogItem> CatalogItems 
        {
            get
            {
                if (catalogItems == null || catalogItems.Count == 0)
                {
                    RebuildConfigableList();
                }
                return catalogItems;
            }
        }

        public IReadOnlyList<string> EntityPaths => entityPaths;

        private void Awake()
        {
            Refresh();
        }

        private void Refresh()
        {
            entityPaths = EditorAssetUtil.FindAsset<ConfigableEntity>("Assets/", true);
        }

        private void OnEnable()
        {
            hasInstance = true;
        }
        private void OnDisable()
        {
            hasInstance = false;
        }

        internal void TryAddAssetPath(string path)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (type.IsSubclassOf(typeof(ConfigableEntity)))
            {
                if (!entityPaths.Contains(path))
                {
                    entityPaths.Add(path);
                }
            }
        }

        public void RebuildConfigableList()
        {
            if (entityPaths.Count == 0)
                Refresh();
            Dictionary<System.Type, int> typeIndex = new Dictionary<System.Type, int>();
            if (catalogItems == null)
                catalogItems = new List<ConfigableCatalogItem>();
            foreach (var path in entityPaths)
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (!typeIndex.TryGetValue(type, out int index))
                {
                    string catalogName = TypeToCatalogName(type);
                    index = catalogItems.FindIndex(it => it.Name == catalogName);
                    if (index < 0)
                    {
                        index = catalogItems.Count;
                        catalogItems.Add(new ConfigableCatalogItem 
                        { 
                            Name = catalogName,
                            Icon = AssetPreview.GetMiniTypeThumbnail(type),
                        });
                    }
                }
                var catalogItem = catalogItems[index];
                if (!catalogItem.Items.Exists(it=>it.Path == path))
                {
                    var item = new ConfigableItem 
                    { 
                        Path = path, 
                        Name = System.IO.Path.GetFileNameWithoutExtension(path),
                    };
                    item.Content = new GUIContent(item.Name, catalogItem.Icon, path);
                    catalogItem.Items.Add(item);
                }
            }
            //清理不存在的
            if (typeIndex.Count < catalogItems.Count)
            {
                var indexs = typeIndex.Values.ToHashSet();
                for (int i=catalogItems.Count-1; i>=0; --i)
                {
                    if (!indexs.Contains(i))
                    {
                        catalogItems.RemoveAt(i);
                    }
                    else
                    {
                        var catalogItem = catalogItems[i];
                        for (int j=catalogItem.Items.Count-1; i>=0; --i)
                        {
                            if (!entityPaths.Contains(catalogItem.Items[j].Path))
                            {
                                catalogItem.Items.RemoveAt(j);
                            }
                        }
                    }
                }
            }
            for (int i=0; i<catalogItems.Count; ++i)
            {
                var catalogItem = catalogItems[i];
                catalogItem.Items.Sort((a, b) => string.Compare(a.Name, b.Name));
                int index = 0;
                foreach (var item in catalogItem.Items)
                {
                    item.CatalogIndex = i;
                    item.IndexInCatalog = index++;
                }
            }
        }
        private string TypeToCatalogName(System.Type type)
        {
            var attr = type.GetCustomAttribute<ConfigableEntityCatalogAttribute>(true);
            if (attr != null)
            {
                return attr.CatalogName;
            }
            return type.Name;
        }
    }

    internal class ConfigableEntityMonitor : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ConfigableEntityCollector.hasInstance)
                return;
            var instance = ConfigableEntityCollector.instance;
            if (string.IsNullOrEmpty("Assets/"))
                return;
            foreach (var asset in importedAssets)
            {
                if (asset.StartsWith("Assets/") && asset.EndsWith(".asset"))
                {
                    instance.TryAddAssetPath(asset);
                }
            }
            foreach (var asset in deletedAssets)
            {
                if (asset.StartsWith("Assets/") && asset.EndsWith(".asset"))
                    instance.entityPaths.Remove(asset);
            }
            foreach (var asset in movedAssets)
            {
                if (asset.StartsWith("Assets/") && asset.EndsWith(".asset"))
                {
                    instance.TryAddAssetPath(asset);
                }
            }
            foreach (var asset in movedFromAssetPaths)
            {
                if (asset.StartsWith("Assets/") && asset.EndsWith(".asset"))
                {
                    instance.entityPaths.Remove(asset);
                }
            }
        }
    }
}
