using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuestObjective))]
public class QuestObjectiveDrawer : TypedPropertyDrawer
{
    protected override string TypePropertyName => "type";

    private static QuestObjectiveType GetObjectiveType(SerializedProperty a_property)
    {
        return (QuestObjectiveType)a_property.FindPropertyRelative("type").intValue;
    }

    protected override IEnumerable<Field> GetFields(SerializedProperty a_property)
    {
        yield return new Field("description", "Description");

        switch (GetObjectiveType(a_property))
        {
            case QuestObjectiveType.TalkTo:
                yield return new Field("target", "NPC ObjectId");
                yield return new Field("requiredAmount", "Times");
                break;
            case QuestObjectiveType.CollectItem:
                yield return new Field("item", "Item");
                yield return new Field("requiredAmount", "Amount");
                break;
            case QuestObjectiveType.Flag:
                yield return new Field("target", "Flag");
                break;
            case QuestObjectiveType.Custom:
                yield return new Field("target", "Key");
                yield return new Field("requiredAmount", "Times");
                break;
        }
    }

    protected override bool HasExtraLine(SerializedProperty a_property)
    {
        return GetObjectiveType(a_property) == QuestObjectiveType.TalkTo;
    }

    // Drag an NPC from the open scene instead of copy pasting its id.
    protected override void DrawExtraLine(Rect a_rect, SerializedProperty a_property)
    {
        SerializedProperty target = a_property.FindPropertyRelative("target");
        ObjectId current = FindObjectId(target.stringValue);

        EditorGUI.BeginChangeCheck();
        GUIContent label = new GUIContent("NPC In Scene", "Drag an NPC from the open scene to fill in its ObjectId.");
        ObjectId picked = (ObjectId)EditorGUI.ObjectField(a_rect, label, current, typeof(ObjectId), true);
        if (EditorGUI.EndChangeCheck())
        {
            target.stringValue = picked != null ? picked.Id : "";
        }
    }

    private static ObjectId FindObjectId(string a_id)
    {
        if (string.IsNullOrEmpty(a_id))
        {
            return null;
        }

        foreach (ObjectId objectId in Object.FindObjectsByType<ObjectId>(FindObjectsSortMode.None))
        {
            if (objectId.Id == a_id)
            {
                return objectId;
            }
        }
        return null;
    }
}
