using UnityEngine;
using System.Collections.Generic;
using TMPro; // For TextMeshProUGUI
using UnityEngine.UI; // For Image

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    //public List<Item> Items = new List<Item>(); // Pre-existing items
    //public Dictionary<Item, int> ItemCounts = new Dictionary<Item, int>();

    public BaseUnit CurrentUnit;

    public Transform ItemContent;
    public GameObject InventoryItem;

    private void Awake()
    {
        Instance = this;

        // Process pre-existing items in the Items list
        //foreach (Item item in Items)
        //{
        //    if (ItemCounts.ContainsKey(item))
        //    {
        //        ItemCounts[item]++;
        //    }
        //    else
        //    {
        //        ItemCounts.Add(item, 1);
        //    }
        //}
    }

    //public void Add(Item item)
    //{
    //    if (ItemCounts.ContainsKey(item))
    //    {
    //        ItemCounts[item]++;
    //    }
    //    else
    //    {
    //        ItemCounts.Add(item, 1);
    //    }
    //}

    //public void Remove(Item item)
    //{
    //    if (ItemCounts.ContainsKey(item))
    //    {
    //        ItemCounts[item]--;
    //        if (ItemCounts[item] <= 0)
    //        {
    //            ItemCounts.Remove(item);
    //        }
    //    }
    //}

    //public void ListItems()
    //{
    //    // Clean content before open
    //    foreach (Transform child in ItemContent)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    foreach (var pair in ItemCounts)
    //    {
    //        GameObject obj = Instantiate(InventoryItem, ItemContent);
    //        var itemName = obj.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
    //        var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();
    //        var itemCount = obj.transform.Find("ItemCount").GetComponent<TextMeshProUGUI>(); // Assuming you have a TextMeshProUGUI component for the item count

    //        itemName.text = pair.Key.itemName;
    //        itemIcon.sprite = pair.Key.icon;
    //        itemCount.text = pair.Value.ToString();
    //    }
    //}

    public void Add(Item item)
    {
        Debug.Log("hi");
        if (BaseUnitManager.Instance.SelectedPlayer == null) return;
        Debug.Log(BaseUnitManager.Instance.SelectedPlayer);
        BaseUnitManager.Instance.SelectedPlayer.UnitInventory.AddItem(item);
    }

    public void Remove(Item item)
    {
        if (BaseUnitManager.Instance.SelectedPlayer == null) return;
        BaseUnitManager.Instance.SelectedPlayer.UnitInventory.RemoveItem(item);
    }

    public void ListItems()
    {
        if (BaseUnitManager.Instance.SelectedPlayer == null) return;

        CurrentUnit = BaseUnitManager.Instance.SelectedPlayer;

        // Clean content before open
        foreach (Transform child in ItemContent)
        {
            Destroy(child.gameObject);
        }

        // Read items from the unit's inventory
        var unitItems = CurrentUnit.UnitInventory.ItemCounts;
        foreach (var pair in unitItems)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            var itemName = obj.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();
            var itemCount = obj.transform.Find("ItemCount").GetComponent<TextMeshProUGUI>();

            itemName.text = pair.Key.itemName;
            itemIcon.sprite = pair.Key.icon;
            itemCount.text = pair.Value.ToString();
        }
    }
}
