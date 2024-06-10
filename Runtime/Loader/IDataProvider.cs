namespace EasyConfig
{
    public interface IDataProvider
    {
        byte[] LoadData(string type, string name);
    }
}
