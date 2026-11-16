using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap decorationTilemap;
    [SerializeField] private Tilemap hazardTilemap;
    [SerializeField] private Tilemap backgroundTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase platformTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase decorationTile;
    [SerializeField] private TileBase backgroundWallTile;
    [SerializeField] private TileBase hazardTile;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Infinite Room Settings")]
    [SerializeField] private int roomWidth = 20;
    [SerializeField] private int roomHeight = 14;
    [SerializeField] private int initialRooms = 3;
    [SerializeField] private int roomsAhead = 2;

    [Header("Platform Settings")]
    [SerializeField] private int platformCountPerRoom = 7;
    [SerializeField] private int minPlatformWidth = 4;
    [SerializeField] private int maxPlatformWidth = 6;
    [SerializeField] private int minPlatformHeight = 4;
    [SerializeField] private int maxPlatformHeight = 10;
    [SerializeField] private int maxPlatformStep = 2;

    [Header("Hazard Settings")]
    [SerializeField] private int hazardCountPerRoom = 2;

    [Header("Decoration Settings")]
    [SerializeField] private int decorationCountPerRoom = 2;

    [Header("Puzzle Settings")]
    [SerializeField] private PuzzleController puzzleController;
    [SerializeField] private GameObject potPrefab;
    [SerializeField] private GameObject switchPrefab;
    [SerializeField] private int requiredSwitches = 4;
    [SerializeField] private int objectiveRoomCount = 8;

    [Header("Switch Placement")]
    [Range(0f, 1f)]
    [SerializeField] private float switchInPotChance = 0.5f;

    [Header("Additional Pot Settings")]
    [SerializeField] private int additionalPotsPerRoom = 3;
    [SerializeField] private int potHazardSafeDistance = 2;

    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private int portalRoomIndex = 9;
    [SerializeField] private float portalOffsetX = 1.5f;

    [Header("Other Objects")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject airEnemyPrefab;
    [SerializeField] private GameObject flowersPrefab;
    [SerializeField] private int enemiesPerRoom = 1;
    [SerializeField] private int airEnemiesPerRoom = 1;
    [SerializeField] private int flowersPerRoom = 1;

    [Header("Hazard Spacing")]
    [SerializeField] private int flowerHazardSafeDistance = 4;
    [SerializeField] private int enemyHazardSafeDistance = 4;

    [Header("Spawn Safety")]
    [SerializeField] private float spawnSafeRadius = 5f;

    [Header("Random Settings")]
    [SerializeField] private int randomSeed = 0;

    [Header("Platform Spacing")]
    [SerializeField] private int minHorizontalGap = 2;
    [SerializeField] private int maxHorizontalGap = 4;
    [SerializeField] private int minVerticalGap = 1;
    [SerializeField] private int maxVerticalGap = 2;

    [Header("Missed Switch Settings")]
    [SerializeField] private float missedSwitchDistance = 1.5f;
    [SerializeField] private int missedSwitchMinX = 5;
    [SerializeField] private int missedSwitchMaxX = 15;

    private int nextRoomIndex = 0;
    private bool levelGenerated = false;
    private bool portalSpawned = false;
    private bool isGame1Scene;

    private GameObject spawnedPortal;
    private int currentPortalRoomIndex = -1;

    private Dictionary<int, Vector3Int> puzzlePositions =
        new Dictionary<int, Vector3Int>();

    private Dictionary<int, GameObject> puzzleCarriers =
        new Dictionary<int, GameObject>();

    private Dictionary<int, GameObject> puzzleSwitchObjects =
        new Dictionary<int, GameObject>();

    private HashSet<Vector3Int> puzzleCells =
        new HashSet<Vector3Int>();

    private HashSet<Vector3Int> occupiedSpawnCells =
        new HashSet<Vector3Int>();

    private void Awake()
    {
        isGame1Scene =
            SceneManager.GetActiveScene().name == "Game1";

        if (!isGame1Scene)
            enabled = false;
    }

    private void Start()
    {
        if (!isGame1Scene)
            return;

        GenerateLevel();
    }

    private void Update()
    {
        if (!isGame1Scene || !levelGenerated || player == null)
            return;

        float generationPoint =
            (nextRoomIndex - roomsAhead) * roomWidth;

        if (player.position.x >= generationPoint)
        {
            GenerateNextRoom();
        }

        CheckMissedPuzzleObjects();
        CheckPortalProgress();
    }

    public void GenerateLevel()
    {
        if (!isGame1Scene)
            return;

        if (groundTilemap == null ||
            wallTilemap == null ||
            floorTile == null ||
            platformTile == null ||
            wallTile == null)
        {
            Debug.LogError(
                "Assign the required Tilemaps, Ground Tile, Platform Tile, and Wall Tile!"
            );
            return;
        }

        if (puzzleController == null)
        {
            Debug.LogError("PuzzleController reference is missing!");
            return;
        }

        if (potPrefab == null || switchPrefab == null)
        {
            Debug.LogError("Assign both the Pot and Switch prefabs!");
            return;
        }

        if (portalPrefab == null)
        {
            Debug.LogError("Portal prefab is missing!");
            return;
        }

        if (randomSeed == 0)
            Random.InitState(System.Environment.TickCount);
        else
            Random.InitState(randomSeed);

        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        if (decorationTilemap != null)
            decorationTilemap.ClearAllTiles();

        if (hazardTilemap != null)
            hazardTilemap.ClearAllTiles();

        if (backgroundTilemap != null)
            backgroundTilemap.ClearAllTiles();

        nextRoomIndex = 0;
        portalSpawned = false;
        levelGenerated = false;

        spawnedPortal = null;
        currentPortalRoomIndex = -1;

        puzzlePositions.Clear();
        puzzleCarriers.Clear();
        puzzleSwitchObjects.Clear();
        puzzleCells.Clear();
        occupiedSpawnCells.Clear();

        puzzleController.SetRequiredSwitches(requiredSwitches);
        puzzleController.ResetPuzzle();

        GeneratePuzzlePositions();

        for (int i = 0; i < initialRooms; i++)
        {
            GenerateNextRoom();
        }

        SpawnPlayer();

        levelGenerated = true;

        Debug.Log("Initial dungeon rooms generated!");
    }

    private void GeneratePuzzlePositions()
    {
        if (objectiveRoomCount < requiredSwitches)
        {
            Debug.LogError(
                "Objective Room Count must be at least " +
                requiredSwitches + "!"
            );
            return;
        }

        List<int> availableRooms = new List<int>();

        for (int i = 1; i <= objectiveRoomCount; i++)
            availableRooms.Add(i);

        for (int i = 0; i < availableRooms.Count; i++)
        {
            int randomIndex = Random.Range(i, availableRooms.Count);

            int temp = availableRooms[i];
            availableRooms[i] = availableRooms[randomIndex];
            availableRooms[randomIndex] = temp;
        }

        for (int switchId = 1; switchId <= requiredSwitches; switchId++)
        {
            int roomIndex = availableRooms[switchId - 1];

            int startX = roomIndex * roomWidth;
            int endX = startX + roomWidth;

            int x = Random.Range(startX + 3, endX - 3);

            Vector3Int cell;

            bool spawnOnPlatform = Random.value < 0.5f;

            if (spawnOnPlatform)
            {
                int minY = Mathf.Max(4, minPlatformHeight + 1);
                int maxY = Mathf.Min(
                    roomHeight - 3,
                    maxPlatformHeight + 1
                );

                int platformSurfaceY =
                    Random.Range(minY, maxY + 1);

                cell = new Vector3Int(x, platformSurfaceY, 0);
            }
            else
            {
                cell = new Vector3Int(x, 2, 0);
            }

            puzzlePositions.Add(switchId, cell);
            puzzleCells.Add(cell);
        }

        Debug.Log(
            "Puzzle locations selected on ground and floating platforms!"
        );
    }

    private void GenerateNextRoom()
    {
        GenerateRoom(nextRoomIndex);

        Debug.Log("Generated Room " + (nextRoomIndex + 1));

        nextRoomIndex++;
    }

    private void GenerateRoom(int roomIndex)
    {
        int startX = roomIndex * roomWidth;
        int endX = startX + roomWidth;

        occupiedSpawnCells.Clear();

        if (backgroundTilemap != null &&
            backgroundWallTile != null)
        {
            for (int x = startX; x < endX; x++)
            {
                for (int y = 0; y <= roomHeight; y++)
                {
                    backgroundTilemap.SetTile(
                        new Vector3Int(x, y, 0),
                        backgroundWallTile
                    );
                }
            }
        }

        for (int x = startX; x < endX; x++)
        {
            groundTilemap.SetTile(
                new Vector3Int(x, 0, 0),
                floorTile
            );

            groundTilemap.SetTile(
                new Vector3Int(x, 1, 0),
                floorTile
            );
        }

        for (int x = startX; x < endX; x++)
        {
            wallTilemap.SetTile(
                new Vector3Int(x, roomHeight, 0),
                wallTile
            );
        }

        if (roomIndex == 0)
        {
            for (int y = 2; y < roomHeight; y++)
            {
                wallTilemap.SetTile(
                    new Vector3Int(startX, y, 0),
                    wallTile
                );
            }
        }

        GeneratePlatforms(startX, roomIndex);
        GeneratePuzzlePlatforms(roomIndex);
        GenerateHazards(startX, roomIndex);
        GenerateDecorations(startX);

        SpawnPuzzleObjects(roomIndex);
        SpawnAdditionalPots(startX, endX, roomIndex);
        SpawnEnemies(startX, endX, roomIndex);
        SpawnAirEnemies(startX, endX, roomIndex);
        SpawnLanterns(startX, endX, roomIndex);

        if (roomIndex == portalRoomIndex)
            SpawnPortal(roomIndex);
    }

    private void GeneratePlatforms(int startX, int roomIndex)
    {
        int safeMinimumX = roomIndex == 0
            ? Mathf.CeilToInt(spawnSafeRadius) + 1
            : 2;

        int[] platformXPositions;

        if (roomIndex == 0)
        {
            platformXPositions = new int[]
            {
                startX + Mathf.Max(6, safeMinimumX),
                startX + 11,
                startX + 16
            };
        }
        else
        {
            platformXPositions = new int[]
            {
                startX + 2,
                startX + 8,
                startX + 14
            };
        }

        int firstHeight = Random.Range(7, 9);

        int[] platformHeights =
        {
            firstHeight,
            firstHeight + Random.Range(1, 3),
            firstHeight + Random.Range(2, 5)
        };

        List<RectInt> placedPlatforms = new List<RectInt>();

        for (int i = 0; i < platformXPositions.Length; i++)
        {
            int platformX = platformXPositions[i];

            int platformY = Mathf.Clamp(
                platformHeights[i],
                3,
                roomHeight - 2
            );

            int width = 4;

            CreatePlatform(platformX, platformY, width);

            placedPlatforms.Add(
                new RectInt(platformX, platformY, width, 1)
            );
        }

        int extraPlatformCount =
            Mathf.Max(0, platformCountPerRoom - 3);

        for (int i = 0; i < extraPlatformCount; i++)
        {
            int width = Random.Range(
                Mathf.Max(3, minPlatformWidth),
                Mathf.Max(minPlatformWidth + 1, maxPlatformWidth + 1)
            );

            int minX = startX + (roomIndex == 0
                ? Mathf.Max(6, safeMinimumX)
                : 1);

            int maxX = startX + roomWidth - width - 1;

            if (maxX < minX)
                continue;

            bool platformCreated = false;

            for (int attempt = 0; attempt < 30; attempt++)
            {
                RectInt referencePlatform =
                    placedPlatforms[Random.Range(
                        0,
                        placedPlatforms.Count
                    )];

                int direction = Random.value < 0.5f ? -1 : 1;

                int horizontalDistance = Random.Range(
                    minHorizontalGap,
                    maxHorizontalGap + 1
                );

                int x = referencePlatform.x +
                    (direction * (
                        referencePlatform.width +
                        horizontalDistance
                    ));

                if (x < minX || x > maxX)
                    continue;

                int verticalDistance = Random.Range(
                    minVerticalGap,
                    maxVerticalGap + 1
                );

                int y = referencePlatform.y +
                    (Random.value < 0.5f
                        ? -verticalDistance
                        : verticalDistance);

                y = Mathf.Clamp(y, 3, roomHeight - 2);

                RectInt candidate = new RectInt(x, y, width, 1);

                if (!IsPlatformPositionAvailable(
                    candidate,
                    placedPlatforms))
                {
                    continue;
                }

                CreatePlatform(x, y, width);
                placedPlatforms.Add(candidate);

                platformCreated = true;
                break;
            }

            if (!platformCreated)
            {
                Debug.Log(
                    "Skipped extra platform because no suitable position was found."
                );
            }
        }

        Debug.Log(
            "Generated floating platforms in Room " +
            (roomIndex + 1)
        );
    }

    private bool IsPlatformPositionAvailable(
        RectInt candidate,
        List<RectInt> placedPlatforms)
    {
        foreach (RectInt existing in placedPlatforms)
        {
            RectInt expanded = new RectInt(
                existing.xMin - 1,
                existing.yMin - 1,
                existing.width + 2,
                existing.height + 2
            );

            if (expanded.Overlaps(candidate))
                return false;
        }

        return true;
    }

    private void GeneratePuzzlePlatforms(int roomIndex)
    {
        foreach (KeyValuePair<int, Vector3Int> entry in puzzlePositions)
        {
            Vector3Int spawnCell = entry.Value;

            if (spawnCell.x / roomWidth != roomIndex)
                continue;

            if (spawnCell.y <= 2)
                continue;

            int platformY = spawnCell.y - 1;
            int platformStartX = spawnCell.x - 2;
            int platformWidth = 5;

            CreatePlatform(
                platformStartX,
                platformY,
                platformWidth
            );

            for (int x = 0; x < platformWidth; x++)
            {
                occupiedSpawnCells.Add(
                    new Vector3Int(platformStartX + x, platformY, 0)
                );
            }
        }
    }

    private void CreatePlatform(int startX, int y, int width)
    {
        for (int x = 0; x < width; x++)
        {
            Vector3Int position =
                new Vector3Int(startX + x, y, 0);

            groundTilemap.SetTile(position, platformTile);
        }
    }

    private void GenerateHazards(int startX, int roomIndex)
    {
        if (hazardTilemap == null || hazardTile == null)
            return;

        int endX = startX + roomWidth;
        int placedHazards = 0;
        int attempts = 0;

        while (placedHazards < hazardCountPerRoom &&
               attempts < hazardCountPerRoom * 10)
        {
            attempts++;

            int hazardX = Random.Range(startX + 2, endX - 2);

            if (roomIndex == 0 &&
                hazardX < startX + spawnSafeRadius)
            {
                continue;
            }

            Vector3Int hazardPosition =
                new Vector3Int(hazardX, 2, 0);

            if (IsNearPuzzlePosition(hazardPosition, 3))
                continue;

            if (hazardTilemap.GetTile(hazardPosition) != null)
                continue;

            hazardTilemap.SetTile(hazardPosition, hazardTile);
            placedHazards++;
        }
    }

    private bool IsNearPuzzlePosition(
        Vector3Int position,
        int horizontalDistance)
    {
        foreach (Vector3Int puzzleCell in puzzleCells)
        {
            bool closeHorizontally =
                Mathf.Abs(position.x - puzzleCell.x)
                <= horizontalDistance;

            bool closeVertically =
                Mathf.Abs(position.y - puzzleCell.y) <= 1;

            if (closeHorizontally && closeVertically)
                return true;
        }

        return false;
    }

    private void GenerateDecorations(int startX)
    {
        if (decorationTilemap == null || decorationTile == null)
            return;

        int endX = startX + roomWidth;

        for (int i = 0; i < decorationCountPerRoom; i++)
        {
            int x = Random.Range(startX + 1, endX - 1);
            int y = Random.Range(3, roomHeight - 1);

            Vector3Int position = new Vector3Int(x, y, 0);

            if (decorationTilemap.GetTile(position) == null)
            {
                decorationTilemap.SetTile(
                    position,
                    decorationTile
                );
            }
        }
    }

    private void SpawnPuzzleObjects(int roomIndex)
    {
        foreach (KeyValuePair<int, Vector3Int> entry in puzzlePositions)
        {
            int switchId = entry.Key;
            Vector3Int cell = entry.Value;

            if (roomIndex != cell.x / roomWidth)
                continue;

            if (puzzleSwitchObjects.ContainsKey(switchId) &&
                puzzleSwitchObjects[switchId] != null)
            {
                continue;
            }

            Vector3 spawnPosition =
                groundTilemap.GetCellCenterWorld(cell);

            GameObject switchObject = Instantiate(
                switchPrefab,
                spawnPosition,
                Quaternion.identity
            );

            Switch spawnedSwitch =
                switchObject.GetComponentInChildren<Switch>();

            if (spawnedSwitch == null)
            {
                Debug.LogError(
                    "Switch prefab needs a Switch component!"
                );

                Destroy(switchObject);
                continue;
            }

            spawnedSwitch.Initialize(
                switchId,
                puzzleController
            );

            puzzleSwitchObjects[switchId] = switchObject;

            bool hideSwitchInPot =
                Random.value < switchInPotChance;

            if (hideSwitchInPot)
            {
                GameObject potObject = Instantiate(
                    potPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                Pot spawnedPot =
                    potObject.GetComponentInChildren<Pot>(true);

                if (spawnedPot != null)
                {
                    spawnedPot.SetHiddenSwitch(switchObject);

                    puzzleCarriers[switchId] = potObject;

                    Debug.Log(
                        "Switch " + switchId +
                        " hidden inside a pot."
                    );
                }
                else
                {
                    Debug.LogError(
                        "Pot prefab is missing the Pot component!"
                    );

                    Destroy(potObject);
                    puzzleCarriers[switchId] = switchObject;
                }
            }
            else
            {
                puzzleCarriers[switchId] = switchObject;

                Debug.Log(
                    "Switch " + switchId +
                    " spawned without a pot."
                );
            }

            occupiedSpawnCells.Add(cell);

            string surface = cell.y <= 2
                ? "Ground"
                : "Floating Platform";

            Debug.Log(
                "Spawned Switch " + switchId +
                " on " + surface +
                " in Room " + (roomIndex + 1)
            );
        }
    }

    private void CheckMissedPuzzleObjects()
    {
        if (puzzleController == null)
            return;

        foreach (KeyValuePair<int, GameObject> entry in puzzleSwitchObjects)
        {
            int switchId = entry.Key;
            GameObject switchObject = entry.Value;

            if (puzzleController.IsSwitchActivated(switchId))
                continue;

            if (switchObject == null)
                continue;

            GameObject carrier = null;

            if (puzzleCarriers.TryGetValue(switchId, out carrier))
            {
                if (carrier == null)
                    carrier = switchObject;
            }
            else
            {
                carrier = switchObject;
            }

            // If the switch is still hidden inside a pot,
            // check the pot position. Otherwise check the switch.
            GameObject objectToMove = carrier;

            if (switchObject.activeInHierarchy)
                objectToMove = switchObject;

            if (objectToMove == null)
                continue;

            if (player.position.x >
                objectToMove.transform.position.x + missedSwitchDistance)
            {
                MoveSwitchToNextRoom(switchId);
            }
        }
    }

    private void MoveSwitchToNextRoom(int switchId)
    {
        if (!puzzlePositions.ContainsKey(switchId))
            return;

        if (!puzzleSwitchObjects.ContainsKey(switchId))
            return;

        GameObject switchObject = puzzleSwitchObjects[switchId];

        if (switchObject == null)
            return;

        GameObject carrier = null;

        if (puzzleCarriers.TryGetValue(switchId, out carrier))
        {
            if (carrier == null)
                carrier = switchObject;
        }
        else
        {
            carrier = switchObject;
        }

        int oldRoom =
            puzzlePositions[switchId].x / roomWidth;

        int newRoom = Mathf.Max(
            nextRoomIndex,
            oldRoom + 1
        );

        int startX = newRoom * roomWidth;

        int minOffset = Mathf.Clamp(
            missedSwitchMinX,
            2,
            roomWidth - 3
        );

        int maxOffset = Mathf.Clamp(
            missedSwitchMaxX,
            minOffset + 1,
            roomWidth - 2
        );

        int x = startX + Random.Range(
            minOffset,
            maxOffset
        );

        Vector3Int newCell = new Vector3Int(x, 2, 0);

        Vector3 newPosition =
            groundTilemap.GetCellCenterWorld(newCell);

        // Move the pot if the switch is still hidden.
        // If the pot has already broken, move the switch itself.
        if (switchObject.activeInHierarchy)
        {
            switchObject.transform.position = newPosition;
            puzzleCarriers[switchId] = switchObject;
        }
        else if (carrier != null)
        {
            carrier.transform.position = newPosition;
        }

        Vector3Int oldCell = puzzlePositions[switchId];

        puzzleCells.Remove(oldCell);
        puzzleCells.Add(newCell);

        puzzlePositions[switchId] = newCell;

        Debug.Log(
            "Player missed Switch " + switchId +
            ". Moving it to Room " + (newRoom + 1)
        );
    }

    private void SpawnAdditionalPots(
        int startX,
        int endX,
        int roomIndex)
    {
        if (potPrefab == null || additionalPotsPerRoom <= 0)
            return;

        int spawnedCount = 0;

        for (int i = 0; i < additionalPotsPerRoom; i++)
        {
            Vector3Int cell;

            if (!TryGetRandomPotSpawnCell(
                startX,
                endX,
                roomIndex,
                out cell))
            {
                continue;
            }

            Vector3 position =
                groundTilemap.GetCellCenterWorld(cell);

            Instantiate(
                potPrefab,
                position,
                Quaternion.identity
            );

            occupiedSpawnCells.Add(cell);
            spawnedCount++;
        }

        Debug.Log(
            "Spawned " + spawnedCount +
            " additional pots in Room " + (roomIndex + 1)
        );
    }

    private bool TryGetRandomPotSpawnCell(
        int startX,
        int endX,
        int roomIndex,
        out Vector3Int result)
    {
        result = Vector3Int.zero;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            bool spawnOnPlatform = Random.value < 0.5f;

            Vector3Int cell;

            if (spawnOnPlatform)
            {
                int x = Random.Range(startX + 2, endX - 2);
                int y = Random.Range(3, roomHeight - 1);

                Vector3Int platformCell =
                    new Vector3Int(x, y, 0);

                if (groundTilemap.GetTile(platformCell) != platformTile)
                    continue;

                cell = platformCell + Vector3Int.up;
            }
            else
            {
                int x = Random.Range(startX + 2, endX - 2);

                cell = new Vector3Int(x, 2, 0);
            }

            if (roomIndex == 0 &&
                cell.x < startX + spawnSafeRadius)
            {
                continue;
            }

            if (puzzleCells.Contains(cell))
                continue;

            if (occupiedSpawnCells.Contains(cell))
                continue;

            if (hazardTilemap != null &&
                hazardTilemap.GetTile(cell) != null)
            {
                continue;
            }

            if (IsTooCloseToHazard(cell, potHazardSafeDistance))
                continue;

            result = cell;
            return true;
        }

        return false;
    }

    private void SpawnEnemies(
        int startX,
        int endX,
        int roomIndex)
    {
        if (enemyPrefab == null)
            return;

        for (int i = 0; i < enemiesPerRoom; i++)
        {
            Vector3Int cell;

            if (!TryGetRandomSpawnCell(
                startX,
                endX,
                roomIndex,
                enemyHazardSafeDistance,
                out cell))
            {
                continue;
            }

            float groundTopY =
                groundTilemap.CellToWorld(
                    new Vector3Int(cell.x, 2, 0)
                ).y;

            Vector3 position =
                groundTilemap.GetCellCenterWorld(cell);

            position.y = groundTopY;

            GameObject spawnedEnemy = Instantiate(
                enemyPrefab,
                position,
                Quaternion.identity
            );

            AdjustEnemyToSurface(spawnedEnemy, groundTopY);

            occupiedSpawnCells.Add(cell);
        }
    }

    private void SpawnAirEnemies(
        int startX,
        int endX,
        int roomIndex)
    {
        if (airEnemyPrefab == null || airEnemiesPerRoom <= 0)
            return;

        List<Vector3Int> platformCells =
            new List<Vector3Int>();

        for (int x = startX + 2; x < endX - 2; x++)
        {
            for (int y = 3; y < roomHeight - 1; y++)
            {
                Vector3Int platformCell =
                    new Vector3Int(x, y, 0);

                if (groundTilemap.GetTile(platformCell) != platformTile)
                    continue;

                Vector3Int spawnCell =
                    platformCell + Vector3Int.up;

                if (puzzleCells.Contains(spawnCell))
                    continue;

                if (occupiedSpawnCells.Contains(spawnCell))
                    continue;

                if (roomIndex == 0 &&
                    x < startX + spawnSafeRadius)
                {
                    continue;
                }

                if (IsTooCloseToHazard(
                    spawnCell,
                    enemyHazardSafeDistance))
                {
                    continue;
                }

                platformCells.Add(platformCell);
            }
        }

        for (int i = 0; i < airEnemiesPerRoom; i++)
        {
            if (platformCells.Count == 0)
                break;

            int randomIndex =
                Random.Range(0, platformCells.Count);

            Vector3Int platformCell = platformCells[randomIndex];

            platformCells.RemoveAt(randomIndex);

            Vector3 position =
                groundTilemap.GetCellCenterWorld(platformCell);

            float platformTopY =
                groundTilemap.CellToWorld(platformCell).y +
                groundTilemap.cellSize.y;

            GameObject spawnedEnemy = Instantiate(
                airEnemyPrefab,
                position,
                Quaternion.identity
            );

            AdjustEnemyToSurface(spawnedEnemy, platformTopY);

            occupiedSpawnCells.Add(
                platformCell + Vector3Int.up
            );

            Debug.Log(
                "Spawned air enemy on floating platform in Room " +
                (roomIndex + 1)
            );
        }
    }

    private void AdjustEnemyToSurface(
        GameObject spawnedEnemy,
        float surfaceY)
    {
        Physics2D.SyncTransforms();

        Collider2D enemyCollider =
            spawnedEnemy.GetComponentInChildren<Collider2D>();

        if (enemyCollider != null)
        {
            float adjustment =
                surfaceY - enemyCollider.bounds.min.y;

            spawnedEnemy.transform.position +=
                Vector3.up * adjustment;

            Physics2D.SyncTransforms();
        }
    }

    private void SpawnLanterns(
        int startX,
        int endX,
        int roomIndex)
    {
        if (flowersPrefab == null)
            return;

        for (int i = 0; i < flowersPerRoom; i++)
        {
            Vector3Int cell;

            if (!TryGetRandomSpawnCell(
                startX,
                endX,
                roomIndex,
                Mathf.Max(flowerHazardSafeDistance, 5),
                out cell))
            {
                continue;
            }

            Vector3 position =
                groundTilemap.GetCellCenterWorld(cell);

            Instantiate(
                flowersPrefab,
                position,
                Quaternion.identity
            );

            occupiedSpawnCells.Add(cell);
        }
    }

    private bool TryGetRandomSpawnCell(
        int startX,
        int endX,
        int roomIndex,
        int minHazardDistance,
        out Vector3Int result)
    {
        result = Vector3Int.zero;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            int x = Random.Range(startX + 3, endX - 3);

            Vector3Int cell = new Vector3Int(x, 2, 0);

            if (roomIndex == 0 &&
                x < startX + spawnSafeRadius)
            {
                continue;
            }

            if (puzzleCells.Contains(cell))
                continue;

            if (occupiedSpawnCells.Contains(cell))
                continue;

            if (hazardTilemap != null &&
                hazardTilemap.GetTile(cell) != null)
            {
                continue;
            }

            if (IsTooCloseToHazard(cell, minHazardDistance))
                continue;

            result = cell;
            return true;
        }

        Debug.LogWarning(
            "Could not find a free spawn position in Room " +
            (roomIndex + 1)
        );

        return false;
    }

    private bool IsTooCloseToHazard(
        Vector3Int spawnCell,
        int minimumDistance)
    {
        if (hazardTilemap == null || minimumDistance <= 0)
            return false;

        for (int x = -minimumDistance; x <= minimumDistance; x++)
        {
            for (int y = -minimumDistance; y <= minimumDistance; y++)
            {
                Vector3Int checkCell =
                    spawnCell + new Vector3Int(x, y, 0);

                if (hazardTilemap.GetTile(checkCell) != null)
                    return true;
            }
        }

        return false;
    }

    private void SpawnPortal(int roomIndex)
    {
        if (portalSpawned || portalPrefab == null)
            return;

        currentPortalRoomIndex = roomIndex;

        int startX = roomIndex * roomWidth;

        Vector3Int portalCell =
            new Vector3Int(startX + roomWidth - 4, 2, 0);

        Vector3 portalPosition =
            groundTilemap.GetCellCenterWorld(portalCell) +
            Vector3.right * portalOffsetX;

        spawnedPortal = Instantiate(
            portalPrefab,
            portalPosition,
            Quaternion.identity
        );

        spawnedPortal.SetActive(false);

        puzzleController.RegisterPortal(spawnedPortal);

        portalSpawned = true;

        Debug.Log(
            "Portal generated in Room " + (roomIndex + 1)
        );
    }

    private void CheckPortalProgress()
    {
        if (!portalSpawned ||
            spawnedPortal == null ||
            !spawnedPortal.activeInHierarchy ||
            player == null)
        {
            return;
        }

        if (player.position.x >
            spawnedPortal.transform.position.x + 1f)
        {
            currentPortalRoomIndex++;

            MovePortalToNextRoom();
        }
    }

    private void MovePortalToNextRoom()
    {
        if (spawnedPortal == null)
            return;

        int startX = currentPortalRoomIndex * roomWidth;

        Vector3Int portalCell =
            new Vector3Int(startX + roomWidth - 4, 2, 0);

        Vector3 portalPosition =
            groundTilemap.GetCellCenterWorld(portalCell) +
            Vector3.right * portalOffsetX;

        spawnedPortal.transform.position = portalPosition;

        Debug.Log(
            "Player passed the portal. Portal moved to Room " +
            (currentPortalRoomIndex + 1)
        );
    }

    private void SpawnPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is missing!");
            return;
        }

        Vector3 spawnPosition = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : new Vector3(2f, 2.5f, 0f);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = new Vector2(
                spawnPosition.x,
                spawnPosition.y
            );

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            player.position = spawnPosition;
        }

        Physics2D.SyncTransforms();
    }
}
