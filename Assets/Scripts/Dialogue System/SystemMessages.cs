using UnityEngine;

/// <summary>
/// Earthbound style messages the game prints in the text box by itself.
/// Tokens: {player} {item} {amount} {quest} {money}
/// </summary>
[System.Serializable]
public class SystemMessages
{
    public string itemReceived = "{player} got the {item}!";
    public string itemGiven = "{player} handed over the {item}.";
    public string inventoryFull = "{player} can't carry any more items.";
    public string moneyReceived = "{player} got ${amount}!";
    public string moneyPaid = "{player} paid ${amount}.";
    public string questStarted = "New quest: {quest}";
    public string questCompleted = "Quest complete: {quest}";

    public static string Format(string a_template, Item a_item = null, int a_amount = 0, Quest a_quest = null)
    {
        if (string.IsNullOrEmpty(a_template))
        {
            return "";
        }

        string result = a_template.Replace("{amount}", a_amount.ToString());
        if (a_item != null)
        {
            result = result.Replace("{item}", a_item.DisplayName);
        }
        if (a_quest != null)
        {
            result = result.Replace("{quest}", a_quest.Title);
        }
        return result;
    }
}
