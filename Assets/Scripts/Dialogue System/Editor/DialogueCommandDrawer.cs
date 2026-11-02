using System.Collections.Generic;
using UnityEditor;

[CustomPropertyDrawer(typeof(DialogueCommand))]
public class DialogueCommandDrawer : TypedPropertyDrawer
{
    protected override string TypePropertyName => "type";

    protected override IEnumerable<Field> GetFields(SerializedProperty a_property)
    {
        switch ((DialogueCommandType)a_property.FindPropertyRelative(TypePropertyName).intValue)
        {
            case DialogueCommandType.GiveItem:
            case DialogueCommandType.TakeItem:
                yield return new Field("item", "Item");
                yield return new Field("amount", "Amount");
                yield return new Field("silent", "Silent");
                break;
            case DialogueCommandType.GiveMoney:
            case DialogueCommandType.TakeMoney:
                yield return new Field("amount", "Amount ($)");
                yield return new Field("silent", "Silent");
                break;
            case DialogueCommandType.StartQuest:
            case DialogueCommandType.CompleteQuest:
                yield return new Field("quest", "Quest");
                yield return new Field("silent", "Silent");
                break;
            case DialogueCommandType.AdvanceQuest:
                yield return new Field("quest", "Quest");
                yield return new Field("key", "Objective Key");
                yield return new Field("amount", "Amount");
                break;
            case DialogueCommandType.SetFlag:
            case DialogueCommandType.ClearFlag:
                yield return new Field("key", "Flag");
                break;
            case DialogueCommandType.PlayDialogue:
                yield return new Field("dialogue", "Dialogue");
                break;
            case DialogueCommandType.PlaySound:
                yield return new Field("sound", "Sound");
                break;
        }
    }
}
