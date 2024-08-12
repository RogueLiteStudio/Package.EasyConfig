using System.Collections.Generic;
using UnityEditor;

namespace EasyConfig
{
    internal class ConfigableEntityClipboard : ScriptableSingleton<ConfigableEntityClipboard>
    {
        [System.Serializable]
        public class ConfigData
        {
            public string TypeGUID;
            public string Name;
            public string Comment;
            public SerializationData Data;
        }
        [UnityEngine.SerializeField]
        private List<ConfigData> configDataClips = new List<ConfigData>();
        public void CopyConfigData(ConfigComponentData config)
        {
            var data = new ConfigData
            {
                TypeGUID = config.Data.GetType().FullName,
                Name = config.Name,
                Comment = config.Comment,
                Data = TypeSerializerHelper.Serialize(config.Data)
            };
            configDataClips.RemoveAll(it => it.TypeGUID == data.TypeGUID);
            configDataClips.Add(data);
        }
        public bool CheckPaste(ConfigComponentData config)
        {
            return configDataClips.Exists(it => it.TypeGUID == config.TypeGUID);
        }
        public void PasteConfigData(ConfigComponentData config, bool onlyProperty)
        {
            int index = configDataClips.FindIndex(it => it.TypeGUID == config.TypeGUID);
            if (index >= 0)
            {
                var data = TypeSerializerHelper.Deserialize(configDataClips[index].Data) as IConfigComponent;
                config.SetData(data);
                if (!onlyProperty)
                {
                    config.Name = configDataClips[index].Name;
                    config.Comment = configDataClips[index].Comment;
                }
            }
        }
    }
}
