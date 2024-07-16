using BuildWizard.Core;
using System;
using System.Collections.Generic;

namespace BuildWizard.Utilities
{
    [Serializable]
    public class DefaultRunnerRepository : IWizardRepository
    {
        private Dictionary<string, object> _data;

        public DefaultRunnerRepository()
        {
            _data = new();
        }

        public void AddData(string key, object resultValue)
        {
            if (_data.ContainsKey(key))
                _data[key] = resultValue;
            else
                _data.Add(key, resultValue);
        }

        public bool ContainsData(string key)
        {
            return _data.ContainsKey(key);
        }

        public void DeleteData(string key)
        {
            if (_data.ContainsKey(key))
                _data.Remove(key);
        }

        public void DisposeRepository()
        {
            _data.Clear();
        }

        public object GetData(string key)
        {
            if (!_data.ContainsKey(key))
                return default;
            return _data[key];
        }

        public T GetData<T>(string key)
        {
            if (!_data.ContainsKey(key))
                return default;
            return (T)_data[key];
        }

    }
}