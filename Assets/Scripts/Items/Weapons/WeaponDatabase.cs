using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public List<Weapon> allWeapons;

    public Weapon GetWeaponByName(string name)
    {
        return allWeapons.Find(weapon => weapon.itemName == name);
    }
}
