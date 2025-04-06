using System.Collections.Generic;
using UnityEngine;
// all pistols and heavy pistols can use pistol whip
// 10 Caliber Slugs deal D2 Criticals per hit against their target, resolved on a D6 of 5+.
public class HeavyPistol : Weapon
{

    //private void OnEnable()
    //{
    //    InitialSetup(".410 Rampage", heavyPistolIcon);
    //}

    public HeavyPistol()  // Constructor
    {
        Ammo = FullClipSize;
    }

    //public override string Name { get; } = ".410 Rampage";
    //public override int Range { get; } = 8;
    //public override int Damage { get; } = 5;
    private int FullClipSize = 6;

    //    public override void Fire(int shots)
    //    {
    //        Ammo -= shots;
    //        Debug.Log(Ammo);
    //    }

    //    public override void Reload()
    //    {
    //        Ammo = FullClipSize;
    //    }

    //    public override int RollForMaxShotCount()
    //    {
    //        return new System.Random().Next(1, 7);
    //    }

    //    public override int RollForHit(int distance)
    //    {
    //        int diceRoll = new System.Random().Next(1, 7);

    //        if (distance <= 2 && diceRoll >= 2) return Damage;
    //        else if (distance <= 4 && diceRoll >= 3) return Damage;
    //        else if (distance <= 6 && diceRoll >= 4) return Damage;
    //        else if (distance <= 8 && diceRoll >= 5) return Damage;
    //        else if (distance > 8 && diceRoll == 6) return Damage;

    //        return 0;
    //    }

    //    public override int NumberOfCritHits(int distance)
    //    {
    //        int numCritAttempts = RollD2();
    //        int critHits = 0;

    //        for (int i = 0; i < numCritAttempts; i++)
    //        {
    //            if (RollForCrit(distance))
    //            {
    //                critHits++;
    //            }
    //        }

    //        return critHits;
    //    }

    //    public override bool RollForCrit(int distance)
    //    {
    //        int diceRoll = new System.Random().Next(1, 7);

    //        if (distance <= 2 && diceRoll >= 4) return true;
    //        if (distance <= 8 && diceRoll >= 5) return true;
    //        if (distance > 9 && diceRoll == 6) return true;

    //        return false;
    //    }
    //}

}

// 1 crit
// D6 # of shots
public class LightPistol : Weapon
{
    public LightPistol()  // Constructor
    {
        Ammo = FullClipSize;
    }

    //public override string Name { get; } = "9mm";
    //public override int Range { get; } = 8;
    //public override int Damage { get; } = 3;
    private int FullClipSize = 12;

    //public override void Fire(int shots)
    //{
    //    Ammo -= shots;
    //    Debug.Log(Ammo);
    //}

    //public override void Reload()
    //{
    //    Ammo = FullClipSize;
    //}

    //public override int RollForMaxShotCount()
    //{
    //    return new System.Random().Next(1, 7);
    //}

    //public override int RollForHit(int distance)
    //{
    //    int diceRoll = new System.Random().Next(1, 7);

    //    if (distance <= 2) return Damage;
    //    if (distance <= 8 && diceRoll >= 3) return Damage;
    //    if (distance <= 14 && diceRoll >= 4) return Damage;
    //    if (distance <= 16 && diceRoll >= 5) return Damage;
    //    if (distance > 17 && diceRoll == 6) return Damage;

    //    return 0;
    //}

    //public override int NumberOfCritHits(int distance)
    //{
    //    // crit attempts should pull from ammo config
    //    int numCritAttempts = 1;
    //    int critHits = 0;

    //    for (int i = 0; i < numCritAttempts; i++)
    //    {
    //        if (RollForCrit(distance))
    //        {
    //            critHits++;
    //        }
    //    }

    //    return critHits;
    //}

    //public override bool RollForCrit(int distance)
    //{
    //    int diceRoll = new System.Random().Next(1, 7);

    //    if (distance <= 2 && diceRoll >= 4) return true;
    //    if (distance <= 8 && diceRoll >= 5) return true;
    //    if (distance > 9 && diceRoll == 6) return true;

    //    return false;
    //}
}