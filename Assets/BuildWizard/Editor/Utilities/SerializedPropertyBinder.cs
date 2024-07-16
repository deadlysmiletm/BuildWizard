using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Diagnostics;

namespace BuildWizard.Utilities
{
    public static class SerializedPropertyBinder
    {
        internal static VisualElement GenerateAndBindPropertyField(string propertyName, Type propertyType, SerializedProperty dataProperty)
        {
            if (propertyType == typeof(int))
            {
                var property = dataProperty.FindPropertyRelative(propertyName);
                var intField = new IntegerField($"{propertyName}:");
                intField.BindProperty(property);
                return intField;
            }

            if (propertyType == typeof(float))
            {
                var property = dataProperty.FindPropertyRelative(propertyName);
                var floatField = new FloatField($"{propertyName}:");
                floatField.value = property.floatValue;
                floatField.BindProperty(property);
                return floatField;
            }

            if (propertyType == typeof(string))
            {
                var property = dataProperty.FindPropertyRelative(propertyName);
                var stringField = new TextField($"{propertyName}:");
                stringField.value = property.stringValue;
                stringField.BindProperty(property);
                return stringField;
            }

            if (propertyType == typeof(bool))
            {
                var property = dataProperty.FindPropertyRelative(propertyName);
                var toggle = new Toggle($"{propertyName}:");
                toggle.value = property.boolValue;
                toggle.BindProperty(property);
                return toggle;
            }

            if (propertyType.IsEnum)
            {
                var enumField = new EnumField(propertyName);
                enumField.BindProperty(dataProperty.FindPropertyRelative(propertyName));
                return enumField;
            }
            return new Label($"Not implemented type for {propertyName}, of type {propertyType}");
        }
    }
}