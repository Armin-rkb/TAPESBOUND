using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/New Item", order = 0)]
public class Item : ScriptableObject, IItem
{
    public new string name;
    public string description;
    public int price;

    // Name shown in game, falls back to the asset name.
    public string DisplayName => string.IsNullOrEmpty(name) ? base.name : name;

    public void Use()
    {
    }
}
