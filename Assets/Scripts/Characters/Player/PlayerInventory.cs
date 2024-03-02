using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<Item> inventoryItems = new List<Item>();
    private const int MAX_INVENTORY_SIZE = 10;

    // Start is called before the first frame update
    void Awake()
    {
        inventoryItems.Capacity = MAX_INVENTORY_SIZE;
    }

    public void AddToInventory(Item a_item)
    {
        if (inventoryItems.Count < MAX_INVENTORY_SIZE)
        {
            inventoryItems.Add(a_item);
            Debug.Log("Added Item: " + a_item.name);
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
    }
}
