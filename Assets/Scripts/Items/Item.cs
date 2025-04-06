using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Create New Item")]

public class Item : ScriptableObject
{
    public int id;
    public string itemName;
    public int value;
    public Sprite icon;
    public int quantity = 1;
}
