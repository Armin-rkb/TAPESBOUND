using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a "type" dropdown and below it only the fields that type uses.
/// </summary>
public abstract class TypedPropertyDrawer : PropertyDrawer
{
    protected struct Field
    {
        public string name;
        public string label;

        public Field(string a_name, string a_label)
        {
            name = a_name;
            label = a_label;
        }
    }

    protected abstract string TypePropertyName { get; }
    protected abstract IEnumerable<Field> GetFields(SerializedProperty a_property);

    // Optional extra line below the fields.
    protected virtual bool HasExtraLine(SerializedProperty a_property) { return false; }
    protected virtual void DrawExtraLine(Rect a_rect, SerializedProperty a_property) {}

    private static float Spacing => EditorGUIUtility.standardVerticalSpacing;
    private static float LineHeight => EditorGUIUtility.singleLineHeight;

    public override float GetPropertyHeight(SerializedProperty a_property, GUIContent a_label)
    {
        float height = LineHeight;
        foreach (Field field in GetFields(a_property))
        {
            SerializedProperty child = a_property.FindPropertyRelative(field.name);
            height += Spacing + (child != null ? EditorGUI.GetPropertyHeight(child, true) : LineHeight);
        }

        if (HasExtraLine(a_property))
        {
            height += Spacing + LineHeight;
        }
        return height + Spacing;
    }

    public override void OnGUI(Rect a_position, SerializedProperty a_property, GUIContent a_label)
    {
        EditorGUI.BeginProperty(a_position, a_label, a_property);

        Rect rect = new Rect(a_position.x, a_position.y, a_position.width, LineHeight);
        EditorGUI.PropertyField(rect, a_property.FindPropertyRelative(TypePropertyName), a_label);
        rect.y += LineHeight + Spacing;

        EditorGUI.indentLevel++;
        foreach (Field field in GetFields(a_property))
        {
            SerializedProperty child = a_property.FindPropertyRelative(field.name);
            if (child == null)
            {
                continue;
            }

            rect.height = EditorGUI.GetPropertyHeight(child, true);
            EditorGUI.PropertyField(rect, child, new GUIContent(field.label, child.tooltip), true);
            rect.y += rect.height + Spacing;
        }

        if (HasExtraLine(a_property))
        {
            rect.height = LineHeight;
            DrawExtraLine(rect, a_property);
        }
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }
}
