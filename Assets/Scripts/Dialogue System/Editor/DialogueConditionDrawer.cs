using System.Collections.Generic;
using UnityEditor;

[CustomPropertyDrawer(typeof(DialogueCondition))]
public class DialogueConditionDrawer : TypedPropertyDrawer
{
    protected override string TypePropertyName => "type";

    protected override IEnumerable<Field> GetFields(SerializedProperty a_property)
    {
        switch ((DialogueConditionType)a_property.FindPropertyRelative(TypePropertyName).intValue)
        {
            case DialogueConditionType.QuestState:
                yield return new Field("quest", "Quest");
                yield return new Field("questState", "Is");
                break;
            case DialogueConditionType.Flag:
                yield return new Field("flag", "Flag Is Set");
                break;
            case DialogueConditionType.HasItem:
                yield return new Field("item", "Has Item");
                yield return new Field("amount", "At Least");
                break;
            case DialogueConditionType.HasMoney:
                yield return new Field("amount", "Has At Least ($)");
                break;
            case DialogueConditionType.InParty:
                yield return new Field("character", "In Party");
                break;
        }
        yield return new Field("not", "Not");
    }
}
