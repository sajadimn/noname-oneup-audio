using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AudioShowIfAttribute))]
public class AudioShowIfDrawer : PropertyDrawer
{
    private AudioShowIfAttribute ShowIf => (AudioShowIfAttribute)attribute;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return IsVisible(property)
            ? EditorGUI.GetPropertyHeight(property, label, true)
            : -EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (IsVisible(property))
            EditorGUI.PropertyField(position, property, label, true);
    }

    private bool IsVisible(SerializedProperty property)
    {
        return AudioFieldVisibility.IsVisible(property, ShowIf.FieldName, ShowIf.Value);
    }
}
