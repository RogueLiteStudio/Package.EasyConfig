using UnityEngine;

namespace EasyConfig
{
    [System.Serializable]
    public class ConfigComponentData : ISerializationCallbackReceiver
    {
        public bool Enabled = true;
        public string Name;
        public string Comment;
        public string TypeGUID => jsonData.TypeGUID;
        [SerializeField]
        [HideInInspector]
        private SerializationData jsonData;
        [System.NonSerialized]
        private IConfigComponent data;
        public IConfigComponent Data
        {
            get
            {
                if (data == null)
                {
                    Deserialize();
                }
                return data;
            }
        }
        public void SetData(IConfigComponent nodeData)
        {
            data = nodeData;
            OnBeforeSerialize();
        }
        public void Deserialize()
        {
            data = TypeSerializerHelper.Deserialize(jsonData) as IConfigComponent;
        }

        public void OnAfterDeserialize()
        {
            data = null;
        }
        public void OnBeforeSerialize()
        {
            if (data != null)
                jsonData = TypeSerializerHelper.Serialize(data);
        }
    }
}
