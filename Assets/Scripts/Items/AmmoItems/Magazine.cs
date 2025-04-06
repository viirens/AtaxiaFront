using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Magazine", menuName = "Items/Magazine")]
public class Magazine : Item
{
    public int capacity;  // Maximum ammo the magazine can hold
    //public WeaponType compatibleWeaponType;
    public List<Ammo> loadedAmmo = new List<Ammo>();  // The actual bullets/shells currently in the magazine

    public int CurrentAmmoCount => loadedAmmo.Count;  // Quick way to check how much ammo is in the magazine

    public void LoadMagazine(Ammo ammo)
    {
        //if (compatibleWeaponType != ammo.compatibleWeaponType)
        //{
        //    Debug.Log("Incompatible ammo type!");
        //    return;
        //}

        int roundsToLoad = Math.Min(capacity - CurrentAmmoCount, ammo.quantity);

        for (int i = 0; i < roundsToLoad; i++)
        {
            loadedAmmo.Add(ammo);  // Here we're just adding the same ammo item multiple times for simplicity.
            // If you have different types of individual bullets with varying properties, 
            // you might want to clone the ammo item or structure this differently.
        }

        ammo.quantity -= roundsToLoad;  // Reduce the quantity in the ammo item

        if (ammo.quantity <= 0)
        {
            // Remove ammo item from player's inventory, as it's empty.
        }
    }
}
