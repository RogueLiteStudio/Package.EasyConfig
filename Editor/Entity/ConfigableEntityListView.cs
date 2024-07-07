using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyConfig
{
    internal class ConfigableItem
    {
        public string Path;
        public string Name;
        public Texture2D Icon;
    }

    internal class ConfigableCatalogItem
    {
        public string Name;
        public List<ConfigableItem> Items;
    }
    public class ConfigableEntityListView : IMGUIContainer
    {

    }
}
