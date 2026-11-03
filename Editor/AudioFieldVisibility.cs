using System;
using UnityEditor;

public static class AudioFieldVisibility
{
    private const string ArrayElement = ".Array.data[";

    public static bool IsVisible(SerializedProperty property, string fieldName, object value)
    {
        if (string.IsNullOrEmpty(fieldName))
            return true;

        SerializedProperty field = property.serializedObject.FindProperty(GetSiblingPath(property.propertyPath, fieldName));
        if (field == null)
            return true;

        switch (field.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return field.boolValue == Convert.ToBoolean(value);
            case SerializedPropertyType.Enum:
                return field.intValue == Convert.ToInt32(value);
            default:
                return true;
        }
    }

    private static string GetSiblingPath(string propertyPath, string fieldName)
    {
        int elementIndex = propertyPath.LastIndexOf(ArrayElement, StringComparison.Ordinal);
        if (elementIndex >= 0 && propertyPath.EndsWith("]", StringComparison.Ordinal))
            propertyPath = propertyPath.Substring(0, elementIndex);

        int dot = propertyPath.LastIndexOf('.');
        return dot < 0 ? fieldName : propertyPath.Substring(0, dot + 1) + fieldName;
    }
}
