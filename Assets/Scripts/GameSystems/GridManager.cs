using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    private PlayerData playerData;

    public GameObject player;
    public GameObject tilePrefab;
    public GameObject decoratedTilePrefab; 
    public GameObject[] decorationPrefabs;

    public int gridWidth = 30; 
    public int gridHeight = 30;
    public float tileSpacing = 1.0f;
    public string tileLayerName = "Tiles";
    public string occupiedTileLayerName = "OccupiedTiles";

    public GameObject[,] grid; 
    public bool[,] occupiedCells; // FOR ROOM GENERATION ONLY, use OccupiedTiles layer or Node's hasPlayer/hasEnemy to mark a tile occupied

    // grid generation settings (edit through UnityEditor)
    public int maxRooms = 8; 
    public int minRoomSize = 4;
    public int maxRoomSize = 7;
    public int bufferSize = 1;

    public List<Room> rooms = new List<Room>();

    // for pathfinding
    public Node[,] nodeGrid;

    // for enemy spawning
    public EnemyManager enemyManager;

    void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
            if (playerData != null)
            {
                // boss floor
                if (playerData.currentFloor == 0)
                {
                    maxRooms = 1;
                }
            }
        }
        grid = new GameObject[gridWidth, gridHeight];
        occupiedCells = new bool[gridWidth, gridHeight];
        nodeGrid = new Node[gridWidth, gridHeight];
        
        GenerateRooms(); // Generate random rooms
        ConnectRooms(); // Connect all rooms with corridors
        GenerateRoomDecorations(); // Generate decorations in rooms
        GenerateGridNodes();
        RandomizePlayerPosition();
        
        enemyManager.SpawnEnemies();
    }

    void GenerateGridNodes()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPoint = new Vector3(x * tileSpacing, 0f, z * tileSpacing);

                bool isWalkable = false;
                if (grid[x, z] != null)
                {
                    // The tile exists, check if it's walkable (not occupied by an obstacle)
                    isWalkable = grid[x, z].layer != LayerMask.NameToLayer(occupiedTileLayerName);
                }

                nodeGrid[x, z] = new Node(isWalkable, worldPoint, x, z);
            }
        }
    }

    void GenerateRooms()
    {

        for (int i = 0; i < maxRooms; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);

            Vector2Int position = GetRandomPosition(roomWidth, roomHeight);

            if (position != Vector2Int.zero)
            {
                Room newRoom = new Room(position.x, position.y, roomWidth, roomHeight);
                rooms.Add(newRoom);

                MarkCellsOccupied(position, roomWidth, roomHeight);

                PlaceRoomOnGrid(newRoom);
            }
            else
            {
                Debug.LogWarning($"Failed to place platform {i + 1} after 100 attempts.");
            }
        }
    }

    Vector2Int GetRandomPosition(int width, int height)
    {
        for (int attempts = 0; attempts < 100; attempts++)  // limit attempts to avoid infinite loop
        {
            int x = Mathf.FloorToInt(Random.Range(bufferSize, (float)(gridWidth - width - bufferSize)));
            int z = Mathf.FloorToInt(Random.Range(bufferSize, (float)(gridHeight - height - bufferSize)));


            // make sure it's not overlapping with other rooms
            if (IsAreaFree(x, z, width, height))
            {
                return new Vector2Int(x, z);
            }
        }

        return Vector2Int.zero;
    }

    bool IsAreaFree(int x, int z, int width, int height)
    {
        for (int i = x - bufferSize; i < x + width + bufferSize; i++)
        {
            for (int j = z - bufferSize; j < z + height + bufferSize; j++)
            {
                if (i < 0 || j < 0 || i >= gridWidth || j >= gridHeight || occupiedCells[i, j])
                {
                    return false;
                }
            }
        }
        return true;
    }

    // only for room generation
    void MarkCellsOccupied(Vector2Int position, int width, int height)
    {
        for (int i = position.x; i < position.x + width; i++)
        {
            for (int j = position.y; j < position.y + height; j++)
            {
                if (i < gridWidth && j < gridHeight)
                {
                    occupiedCells[i, j] = true;
                }
            }
        }
    }

    void PlaceRoomOnGrid(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int z = room.z; z < room.z + room.height; z++)
            {
                // 20% chance -> decorated tile
                GameObject tileToInstantiate = Random.value < 0.2f ? decoratedTilePrefab : tilePrefab;

                Vector3 position = new Vector3(x * tileSpacing, 0f, z * tileSpacing);
                GameObject tile = Instantiate(tileToInstantiate, position, Quaternion.identity);

                // set the parent of the tile for organized hierarchy
                tile.transform.SetParent(transform); // transform -> this object's transform

                tile.name = $"RoomTile_{x}_{z}";

                tile.layer = LayerMask.NameToLayer(tileLayerName);

                grid[x, z] = tile;
            }
        }
    }

    void ConnectRooms()
    {
        List<Room> unconnectedRooms = new List<Room>(rooms);
        Room currentRoom = unconnectedRooms[Random.Range(0, unconnectedRooms.Count)];
        unconnectedRooms.Remove(currentRoom);

        while (unconnectedRooms.Count > 0)
        {
            Room nearestRoom = FindNearestRoom(currentRoom, unconnectedRooms);
            CreateCorridor(currentRoom, nearestRoom);

            currentRoom = nearestRoom;
            unconnectedRooms.Remove(currentRoom);
        }
    }

    Room FindNearestRoom(Room currentRoom, List<Room> roomList)
    {
        Room nearestRoom = null;
        float shortestDistance = float.MaxValue;

        foreach (Room room in roomList)
        {
            float distance = Vector2.Distance(currentRoom.GetCenter(), room.GetCenter());
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestRoom = room;
            }
        }

        return nearestRoom;
    }

    void CreateCorridor(Room fromRoom, Room toRoom)
    {
        Vector2Int start = fromRoom.GetCenter();
        Vector2Int end = toRoom.GetCenter();

        // generate corridor horizontally
        while (start.x != end.x)
        {
            start.x += (int) Mathf.Sign(end.x - start.x);
            PlaceCorridorTile(start.x, start.y);
        }

        // generate corridor vertically
        while (start.y != end.y)
        {
            start.y += (int) Mathf.Sign(end.y - start.y);
            PlaceCorridorTile(start.x, start.y);
        }
    }

    void PlaceCorridorTile(int x, int z)
    {
        // check if there is already a tile
        if (grid[x, z] != null)
        {
            return;
        }

        GameObject tileToInstantiate = Random.value < 0.2f ? decoratedTilePrefab : tilePrefab;

        Vector3 position = new Vector3(x * tileSpacing, 0f, z * tileSpacing);
        GameObject tile = Instantiate(tileToInstantiate, position, Quaternion.identity);

        tile.transform.SetParent(transform);

        tile.name = $"CorridorTile_{x}_{z}";

        tile.layer = LayerMask.NameToLayer(tileLayerName);

        grid[x, z] = tile;
    }

    
    void RandomizePlayerPosition()
    {
        Room randomRoom = rooms[Random.Range(0, rooms.Count)];

        Vector2Int spawnPosition;
        int attempts = 0;

        do
        {
            int randomX = Random.Range(randomRoom.x, randomRoom.x + randomRoom.width);
            int randomZ = Random.Range(randomRoom.z, randomRoom.z + randomRoom.height);
            spawnPosition = new Vector2Int(randomX, randomZ);
            attempts++;
        } while (grid[spawnPosition.x, spawnPosition.y].layer == LayerMask.NameToLayer(occupiedTileLayerName) && attempts < 100);

        // move player to position
        if (player != null && grid[spawnPosition.x, spawnPosition.y] != null)
        {
            Debug.Log($"Random player spawn position: {grid[spawnPosition.x, spawnPosition.y].transform.position}");
            player.transform.position = grid[spawnPosition.x, spawnPosition.y].transform.position + new Vector3(0, 0.5f, 0); // Adjust Y if needed
            nodeGrid[spawnPosition.x, spawnPosition.y].hasPlayer = true;
        }
        else
        {
            Debug.LogError("Player object is not assigned in the GridManager or valid spawn position not found.");
        }
    }

    void GenerateRoomDecorations()
    {
        foreach (Room room in rooms)
        {
            int decorationCount = Mathf.FloorToInt(room.width * room.height * 0.2f);
            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

            for (int i = 0; i < decorationCount; i++)
            {
                Vector2Int decorationPosition;
                int attempts = 0;

                do
                {
                    int randomX = Random.Range(room.x, room.x + room.width);
                    int randomZ = Random.Range(room.z, room.z + room.height);
                    decorationPosition = new Vector2Int(randomX, randomZ);
                    attempts++;
                } while ((occupiedPositions.Contains(decorationPosition) || IsAdjacentToCorridor(decorationPosition) || IsNearbyDecoration(decorationPosition, occupiedPositions)) && attempts < 100);

                if (!occupiedPositions.Contains(decorationPosition) && !IsAdjacentToCorridor(decorationPosition) && !IsNearbyDecoration(decorationPosition, occupiedPositions))
                {
                    Vector3 position = new Vector3(decorationPosition.x * tileSpacing, 0.5f, decorationPosition.y * tileSpacing);
                    Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f); // random rotation (0°, 90°, 180°, 270°)
                    GameObject decoration = Instantiate(decorationPrefabs[Random.Range(0, decorationPrefabs.Length)], position, rotation);

                    decoration.transform.SetParent(transform);

                    occupiedPositions.Add(decorationPosition);

                    grid[decorationPosition.x, decorationPosition.y].layer = LayerMask.NameToLayer(occupiedTileLayerName);
                }
            }
        }
    }

    bool IsAdjacentToCorridor(Vector2Int position)
    {
        int[] offsetX = { -1, 0, 1, 0 };
        int[] offsetZ = { 0, -1, 0, 1 };

        for (int i = 0; i < 4; i++)
        {
            int neighborX = position.x + offsetX[i];
            int neighborZ = position.y + offsetZ[i];

            if (neighborX >= 0 && neighborX < gridWidth && neighborZ >= 0 && neighborZ < gridHeight)
            {
                GameObject neighborTile = grid[neighborX, neighborZ];
                if (neighborTile != null && neighborTile.name.StartsWith("CorridorTile"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsNearbyDecoration(Vector2Int position, HashSet<Vector2Int> occupiedPositions)
    {
        int buffer = 1; // buffer size around each decoration

        for (int x = -buffer; x <= buffer; x++)
        {
            for (int z = -buffer; z <= buffer; z++)
            {
                Vector2Int nearbyPosition = new Vector2Int(position.x + x, position.y + z);
                if (occupiedPositions.Contains(nearbyPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public class Room
    {
        public int x, z, width, height;

        public Room(int x, int z, int width, int height)
        {
            this.x = x;
            this.z = z;
            this.width = width;
            this.height = height;
        }

        public bool OverlapsWith(Room other)
        {
            return x < other.x + other.width &&
                   x + width > other.x &&
                   z < other.z + other.height &&
                   z + height > other.z;
        }

        public Vector2Int GetCenter()
        {
            int centerX = x + width / 2;
            int centerZ = z + height / 2;
            return new Vector2Int(centerX, centerZ);
        }
    }
}
