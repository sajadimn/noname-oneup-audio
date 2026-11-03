using UnityEngine;

public class AudioShowIfAttribute : PropertyAttribute
{
    public string FieldName { get; }
    public object Value { get; }

    public AudioShowIfAttribute(string fieldName) : this(fieldName, true)
    {
    }

    public AudioShowIfAttribute(string fieldName, object value)
#if UNITY_2023_3_OR_NEWER
        : base(true)
#endif
    {
        FieldName = fieldName;
        Value = value;
    }
}
