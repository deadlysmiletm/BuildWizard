using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace BuildWizard.Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DictionaryEntry<TKey, TValue>
    {
        public int HashCode;
        public TKey Key;
        public TValue Value;
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct RepositoryDictionary<TKey, TValue> : ISerializable
        where TKey : notnull, IEquatable<TKey>
        where TValue : notnull
    {



        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}