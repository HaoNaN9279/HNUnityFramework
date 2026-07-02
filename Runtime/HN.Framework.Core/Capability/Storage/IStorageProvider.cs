namespace HN.Framework.Core.Capability
{
    public interface IStorageProvider
    {
        void Save(string key, string value);
        string Load(string key);
        void Delete(string key);
    }
}
