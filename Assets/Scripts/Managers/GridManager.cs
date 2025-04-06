
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Newtonsoft.Json;
using System.IO;
using System;
using UnityEngine.Networking;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public Dictionary<Vector2, IGridObject> _tiles;
    public Dictionary<string, List<Vector2>> mapReference;
    [SerializeField] private int _width, _height;
    public Tile _tilePrefab;
    private Color[] probabilityColors = new Color[5];

    List<Vector2> boundaries = new List<Vector2>();

    public LineRenderer lineRenderer;
    [SerializeField] public LineRenderer lineRenderer2;
    public LayerMask wallLayer;

    public ArrowGenerator arrowGenerator;

    void Awake()
    {
        Instance = this;
        boundaries = GenerateMap();
    }

    void Start()
    {
        //if (lineRenderer == null)
        //{
        //    lineRenderer = GetComponent<LineRenderer>();
        //}

        //probabilityColors[0] = Color.red; // High probability
        //probabilityColors[1] = new Color(1f, 0.5f, 0f); // Orange
        //probabilityColors[2] = Color.yellow; // Medium probability
        //probabilityColors[3] = Color.Lerp(Color.green, Color.yellow, 0.5f); // Light Green
        //probabilityColors[4] = Color.green; // Low probability

        float opacityHigh = 1.0f; // Fully opaque
        float opacityIntermediate = 0.75f; // Moderately transparent
        float opacityLow = 0.25f; // More transparent

        // Low Probability - Very Light Blue, more transparent
        probabilityColors[0] = new Color(0.7f, 0.7f, 1f, opacityLow);

        // Intermediate probabilities - increasing saturation and decreasing transparency
        probabilityColors[1] = new Color(0.55f, 0.55f, 1f, opacityLow + 0.125f);
        probabilityColors[2] = new Color(0.4f, 0.4f, 1f, opacityIntermediate);
        probabilityColors[3] = new Color(0.2f, 0.2f, 0.7f, opacityIntermediate + 0.125f);

        // High Probability - Dark Blue, less transparent
        probabilityColors[4] = new Color(0f, 0f, 0.5f, opacityHigh);

        //foreach (var key in attackProbabilityGradient.colorKeys)
        //{
        //    Debug.Log("Color Key: " + key.color + " Time: " + key.time);
        //}
    }

    public void GenerateGrid()
    {
        GameObject tileParent = new GameObject("tiles");
        _tiles = new Dictionary<Vector2, IGridObject>();
        for (float x = -.5f; x < _width; x += .5f)
        {
            for (float y = -.5f; y < _height; y += .5f)
            {
                IGridObject spawnedTile;
                if (x % 1 == 0 && y % 1 == 0)
                {
                    GameObject spawnedGameObject = (GameObject)Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                    spawnedTile = spawnedGameObject.GetComponent<Tile>();
                }
                else
                {
                    spawnedTile = new Boundary();
                }

                spawnedTile.TileName = $"Tile {x} {y}";
                spawnedTile.coordinate = new Vector3(x, y);
                spawnedTile.isNavigable = true;
                spawnedTile.x = x;
                spawnedTile.y = y;
                //string locationString = FindKeyContainingVector(spawnedTile.coordinate);
                //if (locationString != null) spawnedTile.Location = locationString;

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);

                if (spawnedTile is Tile tile)
                {
                    tile.transform.SetParent(tileParent.transform);
                }
                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        GenerateGraph();
    }

    private void GenerateGraph()
    {
        foreach (KeyValuePair<Vector2, IGridObject> tilePair in _tiles)
        {
            // tile coord
            Vector2 pos = tilePair.Key;
            IGridObject tile = tilePair.Value;

            // [left, leftup, up, upright, right, rightdown, down, downleft]
            List<IGridObject> neighbors = new List<IGridObject>();
            List<Tile> neighborTiles = new List<Tile>();
            List<Vector2> directions = new List<Vector2> { new Vector2(pos.x-.5f, pos.y), new Vector2(pos.x-.5f, pos.y+.5f), new Vector2(pos.x, pos.y+.5f), new Vector2(pos.x+.5f, pos.y+.5f),
                                                   new Vector2(pos.x+.5f, pos.y),  new Vector2(pos.x+.5f, pos.y-.5f), new Vector2(pos.x, pos.y-.5f),  new Vector2(pos.x-.5f, pos.y-.5f) };
            List<Vector2> fullDirections = new List<Vector2> { new Vector2(pos.x-1f, pos.y), new Vector2(pos.x-1f, pos.y+1f), new Vector2(pos.x, pos.y+1f), new Vector2(pos.x+1f, pos.y+1f),
                                                   new Vector2(pos.x+1f, pos.y),  new Vector2(pos.x+1f, pos.y-1f), new Vector2(pos.x, pos.y-1f),  new Vector2(pos.x-1f, pos.y-1f) };

            // construct graph
            foreach (Vector2 direction in directions)
            {
                if (!_tiles.ContainsKey(direction))
                {
                    neighbors.Add(null);
                    continue;
                }

                IGridObject neighbourNode = _tiles[direction];

                if (boundaries.Contains(new Vector2(neighbourNode.x, neighbourNode.y)))
                {
                    neighbourNode.isNavigable = false;
                }
                neighbors.Add(_tiles[direction]);
            }

            // add actual neighbor tiles
            if (Mathf.Approximately(Mathf.Round(pos.x), pos.x) && Mathf.Approximately(Mathf.Round(pos.y), pos.y))
            {

                foreach (Vector2 direction in fullDirections)
                {
                    if (!_tiles.ContainsKey(direction))
                    {
                        neighbors.Add(null);
                        continue;
                    }

                    neighborTiles.Add((Tile)_tiles[direction]);
                }
            }

            tile.Neighbors = neighbors;
            tile.NeighborTiles = neighborTiles;
        }
    }

    public bool isBlocked(Vector2 location)
    {
        foreach (Vector2 coord in boundaries)
        {
            if (coord == location)
            {
                Debug.Log("hit");
                return true;
            }

        }
        return false;
    }

    public List<List<Tile>> BFSRings(Tile centerTile, int range)
    {

        List<List<Tile>> rings = new List<List<Tile>>();
        HashSet<Tile> visited = new HashSet<Tile>();
        Queue<Tile> queue = new Queue<Tile>();

        queue.Enqueue(centerTile);
        visited.Add(centerTile);

        for (int i = 0; i <= range; i++)
        {
            List<Tile> currentRing = new List<Tile>();
            int size = queue.Count;

            for (int j = 0; j < size; j++)
            {
                Tile currentTile = queue.Dequeue();
                currentRing.Add(currentTile);

                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }

            rings.Add(currentRing);
        }

        return rings;
    }

    //public void VisualizeRange(Tile centerTile, int range)
    //{
    //    // deactivate any range tiles already in the scene
    //    DeactivateRings();

    //    List<List<Tile>> rings = BFSRings(centerTile, range);

    //    GameObject overlayParent = new GameObject("tileOverlays");

    //    List<GameObject> overlayObjects = new List<GameObject>();

    //    Color[] ringColors = new Color[] { new Color(182, 34, 3, 0.3f) };

    //    for (int i = 0; i < rings.Count; i++)
    //    {
    //        //Color ringColor = ringColors[i % ringColors.Length];
    //        foreach (Tile tile in rings[i])
    //        {
    //            GameObject overlayObject = new GameObject("Tile Overlay");

    //            SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();

    //            overlayRenderer.color = ringColors[0];

    //            overlayRenderer.sprite = tile._highlight.GetComponent<SpriteRenderer>().sprite;
    //            overlayObject.transform.position = tile.transform.position;

    //            overlayObject.transform.SetParent(overlayParent.transform);

    //            overlayObjects.Add(overlayObject);
    //        }
    //    }
    //}

    //public void VisualizeRange(Tile centerTile, int maxDistance)
    //{

    //    // deactivate any range tiles already in the scene
    //    DeactivateRings();
    //    List<IGridObject> reachableTiles = GameManager.Instance.pathfinding.GetReachableTiles((int)centerTile.x, (int)centerTile.y, maxDistance * 2);
    //    List<IGridObject> attackableTiles = GameManager.Instance.pathfinding.GetAttackRangeTiles((int)centerTile.x, (int)centerTile.y, 4);
    //    Debug.Log(attackableTiles.Count);

    //    GameObject overlayParent = new GameObject("tileOverlays");
    //    List<GameObject> overlayObjects = new List<GameObject>();

    //    //Color overlayColor = new Color(182, 34, 3);


    //    foreach (IGridObject tileObject in reachableTiles)
    //    {
    //        //if (tileObject.x == Mathf.Floor(tileObject.x) && tileObject.y == Mathf.Floor(tileObject.y)) Debug.Log(tileObject.TileName);
    //        Tile tile = tileObject as Tile; // Assuming your IGridObject can be cast to Tile.

    //        if (tile == null) continue;

    //        GameObject overlayObject = new GameObject("Tile Overlay");
    //        SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();

    //        //overlayRenderer.color = overlayColor;
    //        overlayRenderer.sprite = tile._highlight.GetComponent<SpriteRenderer>().sprite;
    //        overlayObject.transform.position = tile.transform.position;
    //        overlayObject.transform.SetParent(overlayParent.transform);
    //        overlayObjects.Add(overlayObject);
    //    }

    //    //foreach (IGridObject tileObject in attackableTiles)
    //    //{
    //    //    //Debug.Log(tileObject.TileName);
    //    //    //if (tileObject.x == Mathf.Floor(tileObject.x) && tileObject.y == Mathf.Floor(tileObject.y)) Debug.Log(tileObject.TileName);
    //    //    Tile tile = tileObject as Tile; // Assuming your IGridObject can be cast to Tile.

    //    //    if (tile == null) continue;

    //    //    GameObject overlayObject = new GameObject("Tile Overlay");
    //    //    SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();

    //    //    overlayRenderer.sprite = tile.attackHighlight;
    //    //    overlayObject.transform.position = tile.transform.position;
    //    //    overlayObject.transform.SetParent(overlayParent.transform);
    //    //    overlayObjects.Add(overlayObject);
    //    //}
    //}

    public float CalculateAttackProbability(Tile centerTile, Tile tile)
    {
        float distance = Vector2.Distance(new Vector2(centerTile.x, centerTile.y), new Vector2(tile.x, tile.y));
        if (distance <= 2) return .5f;
        if (distance <= 8) return .33f;
        if (distance > 9) return .16f;
        else return 0f;
    }

    public Color GetColorForProbability(float probability)
    {
        // Ensure probability is clamped between 0 and 1
        probability = Mathf.Clamp01(probability);

        // Determine the index based on probability
        int index = Mathf.FloorToInt(probability * (probabilityColors.Length - 1));
        return probabilityColors[index];
    }

    public void VisualizeRange(Tile centerTile, int maxDistance)
    {
        // Deactivate any range tiles already in the scene
        DeactivateRings();

        // Retrieve reachable and attackable tiles
        List<IGridObject> reachableTiles = GameManager.Instance.pathfinding.GetReachableTiles((int)centerTile.x, (int)centerTile.y, maxDistance * 2);
        //List<IGridObject> attackableTiles = GameManager.Instance.pathfinding.GetAttackRangeTiles((int)centerTile.x, (int)centerTile.y, 999);

        GameObject overlayParent = new GameObject("tileOverlays");
        List<GameObject> overlayObjects = new List<GameObject>();

        // Iterate over all tiles in _tiles
        foreach (KeyValuePair<Vector2, IGridObject> pair in _tiles)
        {
            Tile tile = pair.Value as Tile; // Attempt to cast the IGridObject to Tile
            if (tile == null) continue; // Skip if the cast fails

            // Skip the tile if it is reachable
            if (reachableTiles.Contains(tile)) continue;

            GameObject overlayObject = new GameObject("Tile Overlay");
            SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();

            // Customize the overlay's appearance here
            // overlayRenderer.color = new Color(182, 34, 3);
            overlayRenderer.sprite = tile._highlight.GetComponent<SpriteRenderer>().sprite;
            overlayObject.transform.position = tile.transform.position;
            overlayObject.transform.SetParent(overlayParent.transform);
            overlayObjects.Add(overlayObject);
        }
    }

    public void VisualizeAttackRange(Tile centerTile)
    {
        // Deactivate any range tiles already in the scene
        DeactivateRings();

        // Retrieve reachable and attackable tiles
        List<IGridObject> attackableTiles = GameManager.Instance.pathfinding.GetAttackRangeTiles((int)centerTile.x, (int)centerTile.y, 999);

        GameObject overlayParent = new GameObject("tileOverlays");
        List<GameObject> overlayObjects = new List<GameObject>();

        foreach (IGridObject tileObject in attackableTiles)
        {
            Tile tile = tileObject as Tile;
            if (tile == null) continue;

            GameObject overlayObject = new GameObject("Tile Overlay");
            SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();

            overlayRenderer.sprite = tile.attackHighlight;
            overlayObject.transform.position = tile.transform.position;
            overlayObject.transform.SetParent(overlayParent.transform);

            // Calculate attack probability for this tile
            float probability = CalculateAttackProbability(centerTile, tile); // Implement this method

            // Map probability to color
            Color gradientColor = GetColorForProbability(probability);
            overlayRenderer.color = gradientColor;

            overlayObjects.Add(overlayObject);
        }

    }


    public void DeactivateRings()
    {
        GameObject overlayParent = GameObject.Find("tileOverlays");
        if (overlayParent != null)
        {
            Destroy(overlayParent);
        }
    }

    public IGridObject GetNodeAtPosition(float x, float y)
    {
        if (_tiles.TryGetValue(new Vector2(x, y), out var tile))
        {
            return tile;
        }

        return null;
    }

    public Tile GetTileAtPosition(float x, float y)
    {
        if (_tiles.TryGetValue(new Vector2(x, y), out var tile))
        {
            if (!(tile is Boundary)) return (Tile)tile;

        }

        return null;
    }

    public string FindKeyContainingVector(Vector2 position)
    {
        foreach (KeyValuePair<string, List<Vector2>> entry in mapReference)
        {
            if (entry.Value.Contains(position))
            {
                return entry.Key; // Return the key where the Vector is found
            }
        }
        return null; // Return null or an empty string if the Vector is not found
    }

    public IGridObject GetPlayerSpawnTile()
    {
        return _tiles[new Vector2(5, 0)];
        //return _tiles.Where(t => t.Key.x < _width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public IGridObject GetSpawnTile()
    {
        //return _tiles[new Vector2(5, 5)];
        //return _tiles.Where(t => t.Key.x > _width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
        List<Tile> tileObjects = _tiles.Values.OfType<Tile>().ToList();

        int randomIndex = UnityEngine.Random.Range(0, tileObjects.Count);
        return (Tile)tileObjects[randomIndex];
    }

    public int GetWidth()
    {
        return this._width;
    }

    public int GetHeight()
    {
        return this._height;
    }

    static string StreamingAssetPathForReal()
    {
#if UNITY_EDITOR
        return "file://" + UnityEngine.Application.dataPath + "/StreamingAssets/";
#elif UNITY_ANDROID
        return "jar:file://" + UnityEngine.Application.dataPath + "!/assets/";
#elif UNITY_IOS
        return "file://" + UnityEngine.Application.dataPath + "/Raw/";
#endif
    }

    public List<Vector2> GenerateMap()
    {
        string filePath = "demoMap.json";
        List<string> map = ReadJsonFromFile(filePath).AsEnumerable().Reverse().ToList();
        List<Vector2> boundaries = new List<Vector2>();
        mapReference = new Dictionary<string, List<Vector2>>();
        //4,8
        //4,9

        for (int i = 0; i < map.Count; i++)
        {
            float y = ((float)i / 2) - .5f;

            for (int j = 0; j < map[i].Length; j++)
            {

                float x = ((float)j / 2) - .5f;
                //Debug.Log(x.ToString() + ',' + y.ToString() + ':' + map[i][j]);
                if (map[i][j] == '#')
                {
                    boundaries.Add(new Vector2(x, y));
                }
                else if (map[i][j] == 'h')
                {
                    AddOrUpdatePosition("Hospital", new Vector2(x, y));
                }
                else
                {
                    AddOrUpdatePosition("Street", new Vector2(x, y));
                }
            }
        }

        return boundaries;
    }

    private void AddOrUpdatePosition(string key, Vector2 position)
    {
        // Check if the key exists, and create a new list if not
        if (!mapReference.ContainsKey(key))
        {
            mapReference[key] = new List<Vector2>();
        }

        // Add the position to the list if it doesn't already exist
        if (!mapReference[key].Contains(position))
        {
            mapReference[key].Add(position);
        }
    }
    //public static List<string> ReadJsonFromFile(string filePath)
    //{
    //    using (StreamReader reader = new StreamReader(filePath))
    //    {
    //        string json = reader.ReadToEnd();
    //        List<string> map = JsonConvert.DeserializeObject<List<string>>(json);
    //        return map;
    //    }
    //}


    public static List<string> ReadJsonFromFile(string fileName)
    {
        string path = GetStreamingAssetsPath(fileName);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // Correctly format the file URL for iOS
            string fileUrl = "file://" + path;
            Debug.Log(fileUrl);
            UnityWebRequest request = UnityWebRequest.Get(fileUrl);
            var operation = request.SendWebRequest();
            while (!operation.isDone) { }

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError($"Error reading file: {request.error}");
                return null;
            }

            string json = request.downloadHandler.text;
            Debug.Log(json);
            List<string> map = JsonConvert.DeserializeObject<List<string>>(json);
            return map;
        }
        else
        {
            string json = File.ReadAllText(path);
            List<string> map = JsonConvert.DeserializeObject<List<string>>(json);
            return map;
        }
    }

    private static string GetStreamingAssetsPath(string fileName)
    {
        string path = "";
#if UNITY_EDITOR || UNITY_STANDALONE
        path = Path.Combine(Application.dataPath, "StreamingAssets", fileName);
#elif UNITY_ANDROID
    path = Path.Combine("jar:file://" + Application.dataPath + "!/assets/", fileName);
#elif UNITY_IOS
    path = Path.Combine(Application.dataPath, "Raw", fileName);
#endif
        return path;
    }






    static bool AreEqual(int[][] array1, int[][] array2)
    {
        if (array1 == null || array2 == null)
            return false;

        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] == null || array2[i] == null)
                return false;

            if (array1[i].Length != array2[i].Length)
                return false;

            if (!array1[i].SequenceEqual(array2[i]))
                return false;
        }

        return true;
    }

    //public void DrawPath(List<IGridObject> path)
    //{
    //    float zOffset = 0.1f;
    //    if (path != null)
    //    {
    //        Debug.Log("hiii");
    //        lineRenderer.positionCount = path.Count;

    //        for (int i = 0; i < path.Count; i++)
    //        {
    //            Vector3 nodePosition = path[i].coordinate;
    //            nodePosition.z -= zOffset;
    //            lineRenderer.SetPosition(i, nodePosition);
    //        }
    //    }
    //}

    public void DrawPath(List<IGridObject> path, int distance)
    {
        float zOffset = 0.1f;

        if (path != null && arrowGenerator != null)
        {
            List<Vector3> pathPoints = new List<Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 nodePosition = path[i].coordinate;
                nodePosition.z -= zOffset;
                pathPoints.Add(nodePosition);
            }

            arrowGenerator.pathPoints = pathPoints;
            arrowGenerator.GenerateArrow(distance * 2);

            // Highlight the tile at the specified distance, if it exists
            //Debug.Log(distance);
            //Debug.Log(path.Count);
            //if ((distance * 2) < path.Count && distance > 0)
            //{
            //    IGridObject lastNode = path[distance * 2];
            //    Tile tile = GetTileAtPosition(Mathf.Floor(lastNode.x), Mathf.Floor(lastNode.y));

            //    if (tile != null)
            //    {
            //        ClearHighlights();

            //        Color overlayColor = new Color(182, 34, 3, 1f);

            //        GameObject overlayObject = new GameObject("Tile Overlay");

            //        SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();

            //        overlayRenderer.color = overlayColor;
            //        overlayRenderer.sprite = tile._highlight.GetComponent<SpriteRenderer>().sprite;

            //        overlayObject.transform.position = tile.transform.position;
            //    }
            //}
        }
    }

    public void ClearHighlights()
    {
        GameObject existingOverlayObject = GameObject.Find("Tile Overlay");

        // If such an object is discovered, proceed to eliminate it.
        if (existingOverlayObject != null)
        {
            Destroy(existingOverlayObject);
        }
    }

    public void ClearPath()
    {
        arrowGenerator.ClearArrow();
        //ClearHighlights();
    }

    public bool CheckLineOfSightAndDrawLine(Vector3 playerPosition, Vector3 enemyPosition)
    {
        float zOffset = 0.5f; // Adjust as needed
        Vector3 startLinePosition = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z - zOffset);
        Vector3 endLinePosition;

        Vector3 direction = enemyPosition - playerPosition;
        float distance = direction.magnitude;
        direction.Normalize();

        // Perform the raycast
        RaycastHit hit;
        bool isLoSClear = true; // Assume LoS is clear initially
        if (Physics.Raycast(playerPosition, direction, out hit, distance, wallLayer))
        {
            // Check if the raycast hits a wall
            if (hit.collider.CompareTag("Wall"))
            {
                // LoS is blocked by a wall, set the line end position to collision point
                endLinePosition = hit.point;
                endLinePosition = new Vector3(endLinePosition.x, endLinePosition.y, endLinePosition.z - zOffset); // Adjust Z as needed
                isLoSClear = false;
            }
            else
            {
                // If no collision with a wall, set line end position to enemy's position
                endLinePosition = new Vector3(enemyPosition.x, enemyPosition.y, enemyPosition.z - zOffset);
            }
        }
        else
        {
            // If raycast doesn't hit anything, set line end position to enemy's position
            endLinePosition = new Vector3(enemyPosition.x, enemyPosition.y, enemyPosition.z - zOffset);
        }

        // Update line renderer
        lineRenderer2.positionCount = 2;
        lineRenderer2.SetPosition(0, startLinePosition);
        lineRenderer2.SetPosition(1, endLinePosition);

        // Change color based on LoS status
        Color lineColor = isLoSClear ? Color.green : Color.red;
        lineRenderer2.startColor = lineColor;
        lineRenderer2.endColor = lineColor;

        return isLoSClear;
    }


    //public void UpdateLineOfSight(Vector3 enemyPosition, Vector3 playerPosition)
    //{
    //    float zOffset = 0.5f; // Adjust this value as needed
    //    Vector3 startLinePosition = new Vector3(enemyPosition.x, enemyPosition.y, enemyPosition.z - zOffset);
    //    Vector3 endLinePosition = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z - zOffset);

    //    bool isLoSClear = HasLineOfSight(startLinePosition, endLinePosition, wallLayer);
    //    // Set the number of points the line renderer uses (start and end)
    //    lineRenderer2.positionCount = 2;

    //    // Set the start and end positions
    //    lineRenderer2.SetPosition(0, startLinePosition);
    //    lineRenderer2.SetPosition(1, endLinePosition);

    //    // Optionally change the color based on whether LoS is clear or not
    //    if (isLoSClear)
    //    {
    //        lineRenderer2.startColor = Color.green;
    //        lineRenderer2.endColor = Color.green;
    //    }
    //    else
    //    {
    //        lineRenderer2.startColor = Color.red;
    //        lineRenderer2.endColor = Color.red;
    //    }
    //}

    //bool HasLineOfSight(Vector3 enemyPosition, Vector3 playerPosition, LayerMask wallLayer)
    //{
    //    Vector3 direction = playerPosition - enemyPosition;
    //    float distance = direction.magnitude;
    //    direction.Normalize();

    //    // Perform the raycast
    //    RaycastHit hit;
    //    if (Physics.Raycast(enemyPosition, direction, out hit, distance, wallLayer))
    //    {
    //        // Check if the raycast hits a wall
    //        if (hit.collider.CompareTag("Wall"))
    //        {
    //            return false; // LoS is blocked by a wall
    //        }
    //    }
    //    return true; // LoS is clear
    //}
}
