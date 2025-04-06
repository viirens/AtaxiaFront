using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class BaseUnitManager : MonoBehaviour, ISubject2
{
    [SerializeField] private EnemyUnitManager EnemyUnitManagerInstance;

    public static BaseUnitManager Instance;
    private List<ScriptableUnit> _unitScriptableObjects;
    private List<BaseUnit> _units = new List<BaseUnit>();
    public List<BaseEnemy> _enemies = new List<BaseEnemy>();
    private List<BasePlayer> _players = new List<BasePlayer>();

    public Material outlineMaterial;
    public Material defaultMaterial;

    public BasePlayer SelectedPlayer;
    public BaseEnemy SelectedEnemy;

    private List<List<IGridObject>> InRangeTiles;

    void Awake()
    {
        Instance = this;
        _unitScriptableObjects = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    void Start()
    {

    }

    private IEnumerable<ScriptableUnit> GetUnits<T>(Faction faction) where T : BaseUnit
    {
        return _unitScriptableObjects.Where(u => u.Faction == faction);
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        return (T)_unitScriptableObjects.Where(u => u.Faction == faction).OrderBy(o => UnityEngine.Random.value).First().UnitPrefab;
    }

    public void AnimateUnitMovement(BaseUnit unit, List<IGridObject> path)
    {
        GridManager.Instance.DeactivateRings();
        StartCoroutine(MoveAlongPathCoroutine(unit, path));
    }

    //private IEnumerator MoveAlongPathCoroutine(BaseUnit unit, List<IGridObject> path)
    //{
    //    const float speed = 3f;
    //    IGridObject destTile = path[^1];
    //    GameObject movingObject = GameObject.FindWithTag("3dPlayer");
    //    for (int i = 0; i < path.Count; i++)
    //    {
    //        Vector3 targetPosition = new Vector3(path[i].x, path[i].y, -0.05f);
    //        while ((targetPosition - movingObject.transform.position).sqrMagnitude > 0.01f)
    //        {
    //            movingObject.transform.position = Vector3.MoveTowards(movingObject.transform.position, targetPosition, speed * Time.deltaTime);
    //            yield return null;
    //        }

    //        movingObject.transform.position = targetPosition;
    //    }
    //    Tile tile = destTile as Tile;
    //    if (tile != null)
    //    {
    //        tile.SetUnit(unit);
    //    }
    //    GridManager.Instance.VisualizeRange(unit.OccupiedTile, unit.Range);
    //    Debug.Log("Movement along path completed");
    //}

    private IEnumerator MoveAlongPathCoroutine(BaseUnit unit, List<IGridObject> path)
    {
        const float speed = 12f;
        IGridObject destTile = path[^1];
        GameObject movingObject = unit.UnitPrefab;
        Animator animator = movingObject.GetComponent<Animator>();
        int isWalkingHash = animator ? Animator.StringToHash("isWalking") : 0;

        Debug.Log("Movement start");
        unit.inMovement = true;
        GridManager.Instance.ClearPath();

        // Start camera follow
        CameraMovementTopDown.Instance.FollowMovingUnit(movingObject.transform, unit);

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 targetPosition = new Vector3(path[i].x, path[i].y, -0.05f);
            if (animator) animator.SetBool(isWalkingHash, true);

            while ((targetPosition - movingObject.transform.position).sqrMagnitude > 0.01f)
            {
                movingObject.transform.position = Vector3.MoveTowards(movingObject.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // wait for the next frame
            }

            movingObject.transform.position = targetPosition;
            if (animator) animator.SetBool(isWalkingHash, false);
        }

        Tile tile = destTile as Tile;
        if (tile != null) tile.SetUnit(unit);

        if (unit is BasePlayer) GridManager.Instance.VisualizeRange(unit.OccupiedTile, unit.Movement);
        Debug.Log("Movement along path completed");
        unit.inMovement = false;

        // Stop camera follow
        CameraMovementTopDown.Instance.StopFollowing();
    }


    public void SpawnPlayers()
    {
        SpawnUnits<BasePlayer>(Faction.player);
        Debug.Log(_players[0]);
    }

    public void SpawnEnemies()
    {
        SpawnUnits<BaseEnemy>(Faction.enemy);
    }

    private void SpawnUnits<T>(Faction faction) where T : BaseUnit
    {
        var unitPrefabs = GetUnits<T>(faction);
        List<Vector2> availableTiles = null;
        string selectedEdge = PlayerPrefs.GetString("SelectedEdge");
        foreach (var unitPrefab in unitPrefabs)
        {

            if (faction == Faction.player)
            {
                // Initialize availableTiles only once for the player faction
                if (availableTiles == null)
                {
                    availableTiles = GetAvailableTilesForEdge(selectedEdge);
                }

                if (availableTiles != null && availableTiles.Count > 0)
                {
                    var tilePosition = availableTiles[UnityEngine.Random.Range(0, availableTiles.Count)];
                    availableTiles.Remove(tilePosition); // Remove the chosen position

                    var selectedTile = GridManager.Instance.GetTileAtPosition(tilePosition.x, tilePosition.y);
                    Vector3 offBoardStartPosition = CalculateOffBoardStartPosition(selectedEdge, selectedTile.transform.position);

                    var spawnedUnit = Instantiate(unitPrefab.UnitPrefab);
                    spawnedUnit.gameObject.SetActive(false);
                    if (spawnedUnit.UnitName == "Scavenger2") spawnedUnit.Init(Resources.Load<Weapon>("Items/Weapons/9mm"));
                    else spawnedUnit.Init(Resources.Load<Weapon>("Items/Weapons/HeavyPistol"));

                    if (selectedTile != null)
                    {
                        StartCoroutine(MoveUnitToTile(spawnedUnit, offBoardStartPosition, selectedTile.transform.position));
                        selectedTile.SetUnit(spawnedUnit);
                    }
                    _units.Add(spawnedUnit);
                    _players.Add(spawnedUnit as BasePlayer);
                    MenuManager.Instance.CreatePlayerSelectButton(spawnedUnit as BasePlayer);
                    spawnedUnit.transform.position = offBoardStartPosition;
                    spawnedUnit.gameObject.SetActive(true);
                }
            }
            else if (faction == Faction.enemy)
            {
                var spawnedUnit = Instantiate(unitPrefab.UnitPrefab);
                var randomSpawnTile = GridManager.Instance.GetSpawnTile();
                Tile tile = randomSpawnTile as Tile;
                if (tile != null)
                {
                    tile.SetUnit(spawnedUnit);
                }
                _units.Add(spawnedUnit);
                _enemies.Add(spawnedUnit as BaseEnemy);
                //SetSelectedEnemy(spawnedUnit as BaseEnemy);
            }
        }
    }

    private Vector3 CalculateOffBoardStartPosition(string edge, Vector3 tilePosition)
    {
        float offset = 5f; // Adjust based on the size of the units and the board
        float zPosition = -0.12f; // Set z-coordinate to -0.12

        switch (edge)
        {
            case "Right":
                return new Vector3(tilePosition.x + offset, tilePosition.y, zPosition);
            case "Top":
                return new Vector3(tilePosition.x, tilePosition.y + offset, zPosition);
            case "Bottom":
                return new Vector3(tilePosition.x, tilePosition.y - offset, zPosition);
            // Add cases for other edges as needed
            default:
                return new Vector3(tilePosition.x, tilePosition.y, zPosition);
        }
    }


    private IEnumerator MoveUnitToTile(BaseUnit unit, Vector3 startPosition, Vector3 endPosition)
    {

        yield return null;

        float duration = 1.0f; // Duration of the movement
        float currentTime = 0f;

        // Set initial off-board position with z-coordinate -0.12
        startPosition.z = -0.12f;
        endPosition.z = -0.12f;

        unit.transform.position = startPosition;
        unit.gameObject.SetActive(true);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            unit.transform.position = Vector3.Lerp(startPosition, endPosition, currentTime / duration);
            yield return null;
        }

        unit.transform.position = endPosition; // Ensure final position is set

        GameManager.Instance.GameInitialized = true;
        StartCoroutine(MenuManager.Instance.FadeCanvasIn(1.0f));
    }

    private List<Vector2> GetAvailableTilesForEdge(string edge)
    {
        var availableTiles = new List<Vector2>();

        switch (edge)
        {
            case "Right":
                for (int y = 13; y <= 22; y++) availableTiles.Add(new Vector2(35, y));
                break;
            case "Top":
                for (int x = 13; x <= 22; x++) availableTiles.Add(new Vector2(x, 35));
                break;
            case "Bottom":
                for (int x = 13; x <= 22; x++) availableTiles.Add(new Vector2(x, 0));
                break;
                // Add logic for any other edges as needed
        }

        return availableTiles;
    }



    public void MoveUnit(BaseUnit unit, Tile destinationTile, List<IGridObject> path)
    {
        // Check if path is valid
        if (path == null)
        {
            Debug.Log("No Path Found");
            return;
        }

        if (unit is BaseEnemy)
        {
            EnemyUnitManagerInstance.MoveEnemy(unit as BaseEnemy, destinationTile, path);
        }
        else
        {
            // Update unit's movement points
            int distanceFromPlayer = (path.Count / 2);

            if (unit.Movement - distanceFromPlayer < 0)
            {
                Debug.Log("Not enough distance remaining this turn");
                //ClearSelectedPlayer();
                return;
            }

            else unit.Movement -= distanceFromPlayer;
            // Update display.
            if (unit is BasePlayer)
            {
                MenuManager.Instance.UpdateObserver(SelectedPlayer.Health, SelectedPlayer.WeaponAmmo);
                //MenuManager.Instance.ShowSelectedWeapon(SelectedPlayer);
            }

            AnimateUnitMovement(unit, path);
        }
    }

    public void SetSelectedPlayer(BasePlayer player, bool moveToPlayer = true)
    {
        //Debug.Log(SelectedPlayer);
        SelectedPlayer = player;
        //MenuManager.Instance.ShowSelectedUnit(player);
        //MenuManager.Instance.ShowSelectedWeapon(player);
        MenuManager.Instance.SelectUnit(player);
        MenuManager.Instance.openInventoryButton.gameObject.SetActive(true);

        // should refactor these to all be through one function
        if (GameManager.Instance.Mode == GameManager.InputMode.Movement) GridManager.Instance.VisualizeRange(SelectedPlayer.OccupiedTile, SelectedPlayer.Movement);
        else if (GameManager.Instance.Mode == GameManager.InputMode.Attack) GridManager.Instance.VisualizeAttackRange(SelectedPlayer.OccupiedTile);
        else GridManager.Instance.DeactivateRings();
        //MenuManager.Instance.ShowSelectedWeapon(SelectedPlayer);

        UpdatePlayerMaterials(SelectedPlayer);

        if (moveToPlayer) CameraMovementTopDown.Instance.MoveToUnit(player.transform.position);
    }

    private void UpdatePlayerMaterials(BasePlayer selectedPlayer)
    {
        foreach (BasePlayer p in _players)
        {
            if (p.UnitPrefab != null)
            {
                SpriteRenderer sr = p.UnitPrefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.material = p == selectedPlayer ? outlineMaterial : defaultMaterial;
                }
            }
        }
    }


    public void SetSelectedEnemy(BaseEnemy enemy)
    {
        SelectedEnemy = enemy;
        MenuManager.Instance.ShowSelectedUnit(enemy);
        MenuManager.Instance.SelectEnemyUnit(enemy);

        CameraMovementTopDown.Instance.MoveToUnit(enemy.transform.position);
    }

    public void ClearSelectedPlayer()
    {
        SelectedPlayer = null;
        MenuManager.Instance.ShowSelectedUnit(SelectedPlayer);
        MenuManager.Instance.openInventoryButton.gameObject.SetActive(false);
        GridManager.Instance.ClearPath();
        GridManager.Instance.DeactivateRings();
    }

    public void IteratePlayer()
    {

        int currentIndex = _players.IndexOf(SelectedPlayer);
        // player not yet selected
        if (currentIndex < 0)
        {
            SetSelectedPlayer(_players[0]);
            return;
        }
        else
        {
            int nextIndex = (currentIndex + 1) % _players.Count;
            SetSelectedPlayer(_players[nextIndex]);
        }

    }

    public void RunPlayerTurn()
    {
        if (SelectedPlayer) SetSelectedPlayer(SelectedPlayer, !GameManager.Instance.FirstTurn);
        else SetSelectedPlayer(_players[0], !GameManager.Instance.FirstTurn);
        if (GameManager.Instance.FirstTurn) GameManager.Instance.FirstTurn = false;
    }

    //public IEnumerator RunEnemyTurn()
    //{
    //    Debug.Log("runenemyturn");

    //    foreach (BaseEnemy enemy in _enemies)
    //    {
    //        SelectedEnemy = enemy;
    //        BasePlayer nearestPlayer = FindClosestPlayer();


    //        Tile closestNeighbortileToPlayer = GetClosestNeighborToPlayer(nearestPlayer.OccupiedTile, enemy.coordinate);
    //        List<IGridObject> path = GameManager.Instance.pathfinding.FindPath((int)enemy.coordinate.x, (int)enemy.coordinate.y, (int)closestNeighbortileToPlayer.coordinate.x, (int)closestNeighbortileToPlayer.coordinate.y);
    //        //Debug.Log("==============PATH==============");
    //        //foreach (IGridObject node in path)
    //        //{
    //        //    Debug.Log(node.TileName);
    //        //}
    //        yield return StartCoroutine(EnemyUnitManager.Instance.EnemyActionsWithDelay(path, nearestPlayer));
    //    }
    //    GameManager.Instance.ChangeState(GameManager.GameState.ResetTurn);
    //}

    //IEnumerator EnemyActionsWithDelay(List<IGridObject> path, BasePlayer nearestPlayer)
    //{
    //    GridManager.Instance.VisualizeRange(SelectedEnemy.OccupiedTile, SelectedEnemy.Range);
    //    //MenuManager.Instance.ShowSelectedUnit(SelectedEnemy);
    //    yield return new WaitForSeconds(1.5f);

    //    // don't need to move if already player adjacent
    //    if (!nearestPlayer.OccupiedTile.NeighborTiles.Contains(SelectedEnemy.OccupiedTile))
    //    {
    //        MoveUnit(SelectedEnemy, (Tile)path[path.Count - 1], path);
    //    }
    //    else Debug.Log("ATTACK");

    //    yield return new WaitForSeconds(2.0f);
    //    GridManager.Instance.DeactivateRings();
    //}

    public Tile GetClosestNeighborToPlayer(Tile tile, Vector3 playerPosition)
    {
        return tile?.NeighborTiles
            .Where(neighbor => (neighbor != null && neighbor is Tile && !neighbor.OccupyingUnit))
            .Aggregate((closest, current) =>
            {
                Tile currentTile = current as Tile;
                Tile closestTile = closest as Tile;

                return (currentTile.transform.position - playerPosition).sqrMagnitude < (closestTile.transform.position - playerPosition).sqrMagnitude ? currentTile : closestTile;
            });
    }

    public BasePlayer FindClosestPlayer()
    {
        BasePlayer closestPlayer = null;
        float closestDistance = Mathf.Infinity;
        Vector3 enemyPosition = SelectedEnemy.transform.position;

        foreach (BasePlayer player in _players)
        {
            float distance = Vector3.Distance(enemyPosition, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public void ResetMovement()
    {

        _units.ForEach(unit =>
        {
            unit.Movement = unit.InitMovement;
        });
    }

    public void DestroyUnit(BaseUnit unit, Faction faction)
    {
        if (faction == Faction.enemy) _enemies.Remove((BaseEnemy)unit);
        else if (faction == Faction.player) _players.Remove((BasePlayer)unit);
        _units.Remove(unit);
        Destroy(unit.gameObject);
    }

    protected List<IObserver2> observers = new List<IObserver2>();

    public void RegisterObserver(IObserver2 observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IObserver2 observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    public void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.UpdateObserver(SelectedPlayer);
        }
    }

    public void ResolvePlayerTurn()
    {
        GridManager.Instance.DeactivateRings();
        GameManager.Instance.ChangeState(GameManager.GameState.EnemyTurn);
    }

}


