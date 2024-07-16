using System;
using System.Buffers;
using System.Linq;
using System.Reflection;

namespace BuildWizard.Utilities
{
    public enum PathType
    {
        InAsset,
        Desktop,
        MyDocuments,
        FullCustom,
    }


    public static class ConfigUtilities
    {
        public static string GetOriginPath(PathType originPath)
        {
            return originPath switch
            {
                PathType.InAsset => UnityEngine.Application.dataPath,
                PathType.Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                PathType.MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _ => ""
            };
        }

        public static Span<Type> GetAllScriptsByInterface<T>()
        {
            AppDomain current = AppDomain.CurrentDomain;
            if (current == null)
                return default;

            Assembly[] allAssemblies = current.GetAssemblies();
            Type stepType = typeof(T);

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            int count = 0;
            Type[] tempArray = ArrayPool<Type>.Shared.Rent(allTypes.Length);

            for (int i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].GetInterfaces().Contains(stepType))
                {
                    tempArray[count] = allTypes[i];
                    count++;
                }
            }
            Span<Type> span = new(tempArray, 0, count);
            ArrayPool<Type>.Shared.Return(tempArray);

            return span;
        }

        public static T CreateNewValueInstance<T>(Type stepType)
        {
            return (T)Activator.CreateInstance(stepType);
        }

        public static string GetRequireStep(object value)
        {
            Type stepType = value.GetType();
            return (string)stepType.GetProperty("RequireSteps").GetValue(value);
        }

        public static Span<(string fieldName, Type fieldType)> GetFields(Type value)
        {
            FieldInfo[] fields = value.GetFields();
            (string, Type)[] data = new (string, Type)[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                data[i] = (fields[i].Name, fields[i].FieldType);
            }

            Span<(string, Type)> span = new(data);
            return span;
        }
    }
}
