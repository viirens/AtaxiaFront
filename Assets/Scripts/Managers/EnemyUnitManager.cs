using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class EnemyUnitManager : MonoBehaviour
{
    public static EnemyUnitManager Instance;
    private BasePlayer FocusedPlayer;

    void Awake()
    {
        Instance = this;
    }

    public void MoveEnemy(BaseEnemy enemy, Tile destinationTile, List<IGridObject> path)
    {
        // Limit the path based on the enemy's movement points
        int distanceFromDest = path.Count / 2;

        // technically this kind of movement behavior is enemy type specific
        if (enemy.Movement - distanceFromDest < 0)
        {
            List<Tile> justTiles = path.OfType<Tile>().ToList();

            Tile lastReachableTile = justTiles.Take(enemy.Movement + 1).Last();

            if (lastReachableTile.OccupyingUnit != null)
            {
                Debug.Log("hit this");
                // Find a neighboring tile with the same distance from the enemy's starting position
                Tile alternativeTile = lastReachableTile.NeighborTiles
                    .OfType<Tile>()
                    .FirstOrDefault(t => t.OccupyingUnit == null && GameManager.Instance.pathfinding.FindPath((int)enemy.coordinate.x, (int)enemy.coordinate.y, (int)t.coordinate.x, (int)t.coordinate.y).Count == enemy.Movement);
                Debug.Log(alternativeTile);
                if (alternativeTile != null)
                {
                    lastReachableTile = alternativeTile;
                }
            }

            // only move remaining distance towards player, and can't be boundary node
            int endIndex = path
                .ToList()
                .FindLastIndex(obj => obj is Tile && (UnityEngine.Object)obj == (UnityEngine.Object)lastReachableTile);

            //Debug.Log(lastReachableTile);
            //Debug.Log(path[endIndex].TileName);
            path = path.Take(endIndex + 1).ToList();
            enemy.Movement = 0;
        }
        else enemy.Movement -= distanceFromDest;


        // Update display
        //MenuManager.Instance.ShowSelectedUnit(enemy);

        BaseUnitManager.Instance.AnimateUnitMovement(enemy, path);
    }

    public IEnumerator RunEnemyTurn()
    {
        Debug.Log("runenemyturn");
        foreach (BaseEnemy enemy in BaseUnitManager.Instance._enemies)
        {
            BaseUnitManager.Instance.SetSelectedEnemy(enemy);
            BasePlayer nearestPlayer = BaseUnitManager.Instance.FindClosestPlayer();


            Tile closestNeighbortileToPlayer = BaseUnitManager.Instance.GetClosestNeighborToPlayer(nearestPlayer.OccupiedTile, enemy.coordinate);
            List<IGridObject> path = GameManager.Instance.pathfinding.FindPath((int)enemy.coordinate.x, (int)enemy.coordinate.y, (int)closestNeighbortileToPlayer.coordinate.x, (int)closestNeighbortileToPlayer.coordinate.y);
            //Debug.Log("==============PATH==============");
            //foreach (IGridObject node in path)
            //{
            //    Debug.Log(node.TileName);
            //}
            yield return StartCoroutine(EnemyActionsWithDelay(path, nearestPlayer));
        }
        GameManager.Instance.ChangeState(GameManager.GameState.ResetTurn);
    }

    public IEnumerator EnemyActionsWithDelay(List<IGridObject> path, BasePlayer nearestPlayer)
    {
        //GridManager.Instance.VisualizeRange(BaseUnitManager.Instance.SelectedEnemy.OccupiedTile, BaseUnitManager.Instance.SelectedEnemy.Range);
        //MenuManager.Instance.ShowSelectedUnit(SelectedEnemy);
        yield return new WaitForSeconds(1.5f);

        // don't need to move if already player adjacent
        if (!nearestPlayer.OccupiedTile.NeighborTiles.Contains(BaseUnitManager.Instance.SelectedEnemy.OccupiedTile))
        {
            BaseUnitManager.Instance.MoveUnit(BaseUnitManager.Instance.SelectedEnemy, (Tile)path[path.Count - 1], path);
        }
        else Debug.Log("ATTACK");

        yield return new WaitForSeconds(2.0f);
        GridManager.Instance.DeactivateRings();
    }
}

