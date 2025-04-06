using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState State;
    public InputMode Mode;
    public bool CameraEnabled = false;
    public bool MovementEnabled = true;
    public bool FirstTurn = true;
    public bool GameInitialized = false;
    public static event Action<GameState> OnGameStateChanged;
    public Pathfinding pathfinding;

    void Awake()
    {
        Instance = this;
        // subscribe to endturn button press event
        ButtonBehaviour.endTurn += ChangeState;
        pathfinding = new Pathfinding();
    }

    private void OnDestroy()
    {
        // unsubscribe 
        ButtonBehaviour.endTurn -= ChangeState;
    }

    void Start()
    {
        ChangeState(GameState.InitBoard);
        ChangeInputMode(InputMode.Movement);
    }

    public void ChangeState(GameState newState)
    {
        State = newState;
        OnGameStateChanged?.Invoke(State);
        Debug.Log("Changestate");
        switch (newState)
        {
            case GameState.InitBoard:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.InitGame:
                BaseUnitManager.Instance.SpawnPlayers();
                BaseUnitManager.Instance.SpawnEnemies();
                //this.DisableUnitVisibility();
                ChangeState(GameState.PlayerTurn);
                break;
            case GameState.PlayerTurn:
                //ChangeInputMode(InputMode.Movement);
                BaseUnitManager.Instance.RunPlayerTurn();
                break;
            case GameState.EnemyTurn:
                StartCoroutine(EnemyUnitManager.Instance.RunEnemyTurn());
                break;
            case GameState.ResetTurn:
                BaseUnitManager.Instance.SpawnEnemies();
                ResetTurn();
                ChangeState(GameState.PlayerTurn);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    public void ChangeInputMode(InputMode newState)
    {
        Mode = newState;
    }

    public enum GameState
    {
        InitBoard,
        InitGame,
        SpawnPlayers,
        SpawnEnemies,
        PlayerTurn,
        EnemyTurn,
        ResetTurn
    }

    public enum InputMode
    {
        Attack,
        Movement,
        Camera
    }

    public void DisableUnitVisibility()
    {
        // Find all game objects with specific tags and add them to "units" layer
        (GameObject.FindGameObjectsWithTag("Player").Concat(GameObject.FindGameObjectsWithTag("Enemy"))).ToList().ForEach(unit => unit.layer = LayerMask.NameToLayer("Units"));
    }

    public void ResetTurn()
    {
        BaseUnitManager.Instance.ResetMovement();
    }
}

