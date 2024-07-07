using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyConfig
{
    public class ConfigableEntityCollector : ScriptableSingleton<ConfigableEntityCollector>
    {
        internal static bool hasInstance = false;
        [SerializeField]
        internal List<string> entityPaths = new List<string>();

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
