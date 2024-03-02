using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/New Item", order = 0)]
public class Item : ScriptableObject, IItem
{
    public new string name;
    public string description;
    public int price;

    public void Use()
    {
    }
}
