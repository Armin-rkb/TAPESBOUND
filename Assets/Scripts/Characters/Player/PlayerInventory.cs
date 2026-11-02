using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Current
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<PlayerInventory>();
            }
            return instance;
        }
    }
    private static PlayerInventory instance;

    [SerializeField] private List<Item> inventoryItems = new List<Item>();
    [Header("Money on hand ($)")]
    [SerializeField] private int money = 0;
    private const int MAX_INVENTORY_SIZE = 10;

    // Static Events
    public delegate void OnInventoryChanged();
    public static event OnInventoryChanged onInventoryChanged;

    public IReadOnlyList<Item> Items => inventoryItems;
    public int Money => money;
    public bool IsFull => inventoryItems.Count >= MAX_INVENTORY_SIZE;

    // Start is called before the first frame update
    void Awake()
    {
        inventoryItems.Capacity = MAX_INVENTORY_SIZE;
    }

    public void AddToInventory(Item a_item)
    {
        TryAddItem(a_item);
    }

    /// <summary>
    /// Add an item, returns false when the inventory is full.
    /// </summary>
    public bool TryAddItem(Item a_item)
    {
        if (a_item == null)
        {
            return false;
        }

        if (IsFull)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        inventoryItems.Add(a_item);
        Debug.Log("Added Item: " + a_item.DisplayName);
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Remove up to a_amount of an item, returns how many were removed.
    /// </summary>
    public int RemoveFromInventory(Item a_item, int a_amount = 1)
    {
        int removed = 0;
        while (removed < a_amount && inventoryItems.Remove(a_item))
        {
            removed++;
        }

        if (removed > 0)
        {
            onInventoryChanged?.Invoke();
        }
        return removed;
    }

    public int CountItem(Item a_item)
    {
        if (a_item == null)
        {
            return 0;
        }

        int count = 0;
        foreach (Item item in inventoryItems)
        {
            if (item == a_item)
            {
                count++;
            }
        }
        return count;
    }

    public bool HasItem(Item a_item, int a_amount = 1)
    {
        return CountItem(a_item) >= a_amount;
    }

    public void AddMoney(int a_amount)
    {
        if (a_amount == 0)
        {
            return;
        }

        money = Mathf.Max(0, money + a_amount);
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Pay money, returns false (and pays nothing) when there isn't enough.
    /// </summary>
    public bool SpendMoney(int a_amount)
    {
        if (money < a_amount)
        {
            return false;
        }

        AddMoney(-a_amount);
        return true;
    }
}
