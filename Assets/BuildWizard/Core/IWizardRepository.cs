
namespace BuildWizard.Core
{
    public interface IWizardRepository
    {
        public void AddData(string key, object resultValue);
        public object GetData(string key);
        public T GetData<T>(string key);
        public void DeleteData(string key);
        public bool ContainsData(string key);
        public void DisposeRepository();
    }
}