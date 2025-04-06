using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IGridObject
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] public GameObject _highlight;
    [SerializeField] public Sprite attackHighlight;
    [SerializeField] private bool _isWalkable;

    public List<IGridObject> Neighbors { get; set; }
    public List<Tile> NeighborTiles { get; set; }

    public BaseUnit OccupyingUnit;

    public UnityEngine.Vector3 coordinate { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public string Location { get; set; }
    [SerializeField] private string tileName;

    public string TileName
    {
        get { return tileName; }
        set { tileName = value; }
    }

    public bool isNavigable { get; set; }
    public float gCost { get; set; }
    public float hCost { get; set; }
    public float fCost { get; set; }

    //temp
    private TextMeshProUGUI debugText;
    public IGridObject cameFromNode { get; set; }

    public bool Walkable => _isWalkable && OccupyingUnit == null;

    public Tile()
    {
    }

    public void Init(bool isOffset)
    {
        //_renderer.color = isOffset ? _offsetColor : _baseColor;
    }

    void OnMouseEnter()
    {
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    return;
        //}
        if (!(GameManager.Instance.Mode == GameManager.InputMode.Camera) && GameManager.Instance.GameInitialized)
        {
            BaseUnit player = BaseUnitManager.Instance.SelectedPlayer;
            if (GameManager.Instance.State == GameManager.GameState.PlayerTurn && player && !player.inMovement)
            {
                List<IGridObject> path = GameManager.Instance.pathfinding.FindPath(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y), (int)x, (int)y);
                //GridManager.Instance.DrawPath(path, player.Movement);  
            }
            if (BaseUnitManager.Instance.SelectedPlayer) _highlight.SetActive(true);
        }
        //MenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit()
    {
        GridManager.Instance.ClearPath();
        _highlight.SetActive(false);
        //MenuManager.Instance.ShowTileInfo(null);
    }

    //void OnTouchEnter(UnityEngine.Vector3 touchPos)
    //{
    //    //Debug.Log("enter");
    //    BaseUnit player = BaseUnitManager.Instance.SelectedPlayer;
    //    if (GameManager.Instance.State == GameManager.GameState.PlayerTurn && player && !player.inMovement)
    //    {
    //        List<IGridObject> path = GameManager.Instance.pathfinding.FindPath(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y), (int)x, (int)y);
    //        //GridManager.Instance.DrawPath(path);
    //    }
    //    _highlight.SetActive(true);
    //}

    //void OnTouchExit()
    //{
    //    Debug.Log("exit");
    //    // (Your original OnMouseExit code)
    //    //GridManager.Instance.ClearPath();
    //    GameObject textObject = GameObject.FindGameObjectWithTag("Debug");
    //    debugText = textObject.GetComponent<TextMeshProUGUI>();
    //    debugText.text = "hit";
    //    _highlight.SetActive(false);
    //    //MenuManager.Instance.ShowTileInfo(null);
    //}

    public void SetUnit(BaseUnit unit)
    {
        if (unit.OccupiedTile != null) unit.OccupiedTile.OccupyingUnit = null;
        unit.transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y, -.12f);
        OccupyingUnit = unit;
        unit.OccupiedTile = this;
        unit.coordinate = coordinate;
        unit.NotifyObservers();
    }

    void OnMouseDown()
    {
        if (!(GameManager.Instance.Mode == GameManager.InputMode.Camera))
        {
            // This line checks if the left mouse button was clicked over a UI element.
            // If the current event is a pointer event and it was handled by a GameObject, then we ignore it.
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count > 0)
            {
                foreach (var go in raycastResults)
                {
                    if (go.gameObject.name.Contains("Button")) return;
                }
            }


            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //    return;
            //}
            HandleTileClick(this);
        }
    }

    public void HandleTileClick(Tile tile)
    {
        BasePlayer SelectedPlayer = BaseUnitManager.Instance.SelectedPlayer;

        if (GameManager.Instance.State != GameManager.GameState.PlayerTurn) return;

        if (tile.OccupyingUnit != null)
        {
            if (tile.OccupyingUnit.Faction == Faction.enemy && GameManager.Instance.Mode == GameManager.InputMode.Attack) HandleEnemyTileClick(tile);
            else if (tile.OccupyingUnit.Faction == Faction.player) BaseUnitManager.Instance.SetSelectedPlayer((BasePlayer)tile.OccupyingUnit);
        }
        else
        {
            if (SelectedPlayer != null) HandleEmptyTileClick(tile);
        }
    }

    private void HandleEnemyTileClick(Tile tile)
    {
        BasePlayer SelectedPlayer = BaseUnitManager.Instance.SelectedPlayer;
        if (SelectedPlayer == null) return;

        var hasLOS = GridManager.Instance.CheckLineOfSightAndDrawLine(SelectedPlayer.coordinate, coordinate);
        if (!hasLOS)
        {
            Debug.Log("No line of sight");
            return;
        }
        var enemy = (BaseEnemy)tile.OccupyingUnit;
        var player = BaseUnitManager.Instance.SelectedPlayer;
        List<IGridObject> path = GameManager.Instance.pathfinding.FindPath((int)enemy.coordinate.x, (int)enemy.coordinate.y, (int)player.coordinate.x, (int)player.coordinate.y);
        int distanceFromPlayer = (path.Count / 2);
        BaseUnitManager.Instance.SelectedPlayer.TempAttack(enemy, distanceFromPlayer);
        BaseUnitManager.Instance.SetSelectedEnemy(enemy);
    }

    private void HandleEmptyTileClick(Tile destinationTile)
    {
        BasePlayer SelectedPlayer = BaseUnitManager.Instance.SelectedPlayer;
        if (SelectedPlayer != null && !SelectedPlayer.inMovement && GameManager.Instance.Mode == GameManager.InputMode.Movement)
        {
            //tempPlayer = GameObject.FindWithTag("3dPlayer");

            List<IGridObject> path = GameManager.Instance.pathfinding.FindPath(Mathf.RoundToInt(SelectedPlayer.transform.position.x), Mathf.RoundToInt(SelectedPlayer.transform.position.y), (int)x, (int)y);
            //GridManager.Instance.DrawPath(path);
            //Debug.Log("==============PATH==============");
            //foreach (IGridObject tile in path)
            //{
            //    Debug.Log(tile.TileName);
            //}
            BaseUnitManager.Instance.MoveUnit(SelectedPlayer, destinationTile, path);
        }
    }

    public static explicit operator GameObject(Tile tile)
    {
        if (tile == null)
        {
            return null;
        }

        return tile.gameObject;
    }
}

