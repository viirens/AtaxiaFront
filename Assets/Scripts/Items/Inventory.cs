using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<Item> Items = new List<Item>();
    public Dictionary<Item, int> ItemCounts = new Dictionary<Item, int>();
    public Weapon EquippedWeapon { get; private set; }

    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
    }

    public void AddItem(Item item)
    {
        if (ItemCounts.ContainsKey(item))
        {
            ItemCounts[item] += item.quantity;  // Increment by the item's quantity
        }
        else
        {
            Items.Add(item);
            ItemCounts.Add(item, item.quantity);  // Use the item's quantity here
        }
    }

    public void RemoveItem(Item item)
    {
        if (ItemCounts.ContainsKey(item))
        {
            ItemCounts[item] -= item.quantity;  // Decrement by the item's quantity

            if (ItemCounts[item] <= 0)
            {
                ItemCounts.Remove(item);
                Items.Remove(item);
            }
        }
    }
}

