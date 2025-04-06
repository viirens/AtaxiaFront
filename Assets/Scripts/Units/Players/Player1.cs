using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1 : BasePlayer
{
    private int initMovement = 12;
    private int movement = 12;
    private int range = 3;
    private int actions = 2;

    public override int Range { get { return range; } set { range = value; } }
    public override int InitMovement { get { return initMovement; } set { initMovement = value; } }
    public override int Movement { get { return movement; } set { movement = value; } }
    public override int Actions { get { return actions; } set { actions = value; } }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
