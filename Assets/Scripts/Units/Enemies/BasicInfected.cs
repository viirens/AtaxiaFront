using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicInfected : BaseEnemy
{
    private int initMovement = 6;
    private int movement = 6;
    private int range = 1;


    public override int Range { get { return range; } set { range = value; } }
    public override int InitMovement { get { return initMovement; } set { initMovement = value; } }
    public override int Movement { get { return movement; } set { movement = value; } }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public BasicInfected()
    {
        Crits = 1;
    }

    public int MeleeAttack()
    {
        int roll = new System.Random().Next(1, 7);
        if (roll == 1)
        {
            Debug.Log($"{this.UnitName} used Bite");
            return Bite();
        }
        else if (roll >= 2 && roll <= 4)
        {
            Debug.Log($"{this.UnitName} used Scratch");
            return Scratch();
        }
        else
        {
            Debug.Log($"{this.UnitName} used Collide");
            return Collide();
        }

    }

    int Bite()
    {
        return 4;
    }

    int Scratch()
    {
        return 3;
    }

    int Collide()
    {
        return 2;
    }
}

