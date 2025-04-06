using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item Item;

    void Pickup()
    {
        BaseUnitManager.Instance.SelectedPlayer.UnitInventory.AddItem(Item);
        Debug.Log(BaseUnitManager.Instance.SelectedPlayer.UnitInventory.ItemCounts.Count);
        //InventoryManager.Instance.Add(Item);
        Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        Pickup();
    }
}
