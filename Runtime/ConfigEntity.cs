using System.Collections.Generic;

namespace EasyConfig
{
    public class ConfigEntity
    {
        public int ID;
        public List<IConfigComponent> Components = new List<IConfigComponent>();

        public T Get<T>() where T : class, IConfigComponent
        {
            foreach (var comp in Components)
            {
                if (comp is T data)
                {
                    return data;
                }
            }
            return null;
        }
    }
}
