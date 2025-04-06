using System.Collections.Generic;
using UnityEngine;

//public enum WeaponType
//{
//    Pistol,
//    Rifle,
//    Shotgun
//}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    public virtual string Name { get; set; } = "Default Weapon";
    public int Range;
    public int Damage;

    private int FullClipSize = 6;
    public Magazine loadedMagazine;

    private int ammo;  // private backing field for Ammo property
    public int Ammo
    {
        get { return ammo; }
        set
        {
            if (ammo != value)
            {
                ammo = value;
            }
        }
    }
    public Ammo loadedAmmo;
    public Weapon()
    {
        ammo = FullClipSize; // Initialize ammo to full clip size by default
    }

    public void Fire(int shots)
    {
        if (loadedMagazine != null && loadedMagazine.CurrentAmmoCount >= shots)
        {
            for (int i = 0; i < shots; i++)
            {
                loadedMagazine.loadedAmmo.RemoveAt(loadedMagazine.loadedAmmo.Count - 1);  // Remove the last bullet/shell
            }

            if (loadedMagazine.CurrentAmmoCount <= 0)
            {
                loadedMagazine = null;
            }

            Debug.Log(loadedMagazine.CurrentAmmoCount);
        }
        else
        {
            Debug.Log("Not enough ammo or no magazine loaded!");
        }
    }

    public bool Reload(Magazine magazineToLoad)
    {
        //if (magazineToLoad.compatibleWeaponType == /*current weapon type*/)
        {
            loadedMagazine = magazineToLoad;
            return true;
            //}
            //return false;
        }
    }

    public int RollForMaxShotCount()
    {
        return new System.Random().Next(1, 7);
    }

    public int RollForHit(int distance)
    {
        int diceRoll = new System.Random().Next(1, 7);

        if (distance <= 2 && diceRoll >= 2) return Damage;
        else if (distance <= 4 && diceRoll >= 3) return Damage;
        else if (distance <= 6 && diceRoll >= 4) return Damage;
        else if (distance <= 8 && diceRoll >= 5) return Damage;
        else if (distance > 8 && diceRoll == 6) return Damage;

        return 0;
    }

    public int NumberOfCritHits(int distance)
    {
        int numCritAttempts = RollD2();
        int critHits = 0;

        for (int i = 0; i < numCritAttempts; i++)
        {
            if (RollForCrit(distance))
            {
                critHits++;
            }
        }

        return critHits;
    }

    public bool RollForCrit(int distance)
    {
        int diceRoll = new System.Random().Next(1, 7);

        if (distance <= 2 && diceRoll >= 4) return true;
        if (distance <= 8 && diceRoll >= 5) return true;
        if (distance > 9 && diceRoll == 6) return true;

        return false;
    }

    public int RollD2()
    {
        return new System.Random().Next(1, 3);
    }
}