using UnityEngine;
using System.Collections.Generic;
using static GridManager;
using System.Collections;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public GridManager gridManager; 
    public PlayerData playerData;
    public List<EnemyController> spawnedEnemies = new List<EnemyController>();

    public UnityEvent<bool> OnAllEnemiesDefeated;

    public string[] enemyNames = new string[]
    {
        "AC", "AS", "BD", "BT", "CG",
        "CT", "CV", "DD", "DO", "FO",
        "FR", "FW", "GN", "GY", "HO",
        "JK", "KH", "MJ", "MM", "MR",
        "MV", "NB", "NE", "NS", "NT",
        "OV", "PL", "RU", "SC", "TI",
        "VD", "VM", "VX", "WS", "WW",
        "YD"
    };

    void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
            Debug.Log("PlayerDataSO initialized: " + (playerData != null));
        }
        else
        {
            Debug.LogError("PlayerDataManager instance not found!");
            return;
        }
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        // SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        int enemyCount = Mathf.Clamp(Mathf.FloorToInt(playerData.currentFloor * 0.2f), 1, 20);

        for (int i = 0; i < enemyCount; i++)
        {
            Room randomRoom = gridManager.rooms[Random.Range(0, gridManager.rooms.Count)];
            Vector2Int spawnPosition = GetRandomSpawnPositionInRoom(randomRoom);

            EnemyType type = DetermineEnemyType();
            Enemy newEnemy = GenerateEnemy(type, spawnPosition);

            // instantiate the enemy at the random spawn position
            GameObject enemyGO = Instantiate(enemyPrefabs[(int)type], new Vector3(spawnPosition.x, 0.5f, spawnPosition.y), Quaternion.identity);
            enemyGO.name = newEnemy.name;
            enemyGO.tag = "Enemy";
            EnemyController enemyController = enemyGO.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Initialize(newEnemy);
                enemyController.transform.SetParent(transform);
                enemyController.enemyModel = enemyGO.transform;
                enemyController.enemyManager = this;
                spawnedEnemies.Add(enemyController);
            }
            else
            {
                Debug.LogError("Enemy prefab is missing the EnemyController script.");
            }
        }
    }

    EnemyType DetermineEnemyType()
    {
        // if boss floor
        if (playerData.currentFloor == 0)
        {
            return EnemyType.Boss;
        }

        float randomValue = Random.Range(0f, 1f);
        float floorFactor = Mathf.Clamp01(playerData.currentFloor / 100f);

        // calculate dynamic probabilities based on the current floor
        float commonProbability = Mathf.Lerp(0.7f, 0.3f, floorFactor);  // decreases from 70% to 30%
        float mediumProbability = Mathf.Lerp(0.2f, 0.4f, floorFactor);  // increases from 20% to 40%
        float eliteProbability = Mathf.Lerp(0.1f, 0.3f, floorFactor);   // increases from 10% to 30%

        if (randomValue < commonProbability) 
            return EnemyType.Common;
        else if (randomValue < commonProbability + mediumProbability)
            return EnemyType.Medium;
        else 
            return EnemyType.Elite;
    }


    Enemy GenerateEnemy(EnemyType type, Vector2Int spawnPosition)
    {
        string name = enemyNames[Random.Range(0, enemyNames.Length)];
        int baseHealth = 10 + playerData.currentFloor * 2;
        int baseAttack = 3 + playerData.currentFloor * 2; 
        int baseDefense = 2 + Mathf.FloorToInt(playerData.currentFloor * 0.2f);

        // low impact defense
        int defenseScalingFactor = Random.Range(50, 100);

        switch (type)
        {
            case EnemyType.Common:
                break;

            case EnemyType.Medium:
                baseHealth += 20;
                baseAttack += 5;
                break;

            case EnemyType.Elite:
                baseHealth += 50;
                baseAttack += 10;
                baseDefense += 5;
                break;

            case EnemyType.Boss:
                baseHealth += 1000;
                baseAttack += 100;
                baseDefense += 20;
                break;
        }

        return new Enemy(name, type, baseHealth, baseAttack, baseDefense, defenseScalingFactor);
    }

    public Vector2Int GetRandomSpawnPositionInRoom(Room room)
    {
        Vector2Int spawnPosition = Vector2Int.zero;
        int attempts = 0;
        const int maxAttempts = 100;

        do
        {
            int randomX = Random.Range(room.x, room.x + room.width);
            int randomZ = Random.Range(room.z, room.z + room.height);
            spawnPosition = new Vector2Int(randomX, randomZ);
            attempts++;

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Failed to find valid spawn position after 100 attempts.");
                break;
            }

        } while (gridManager.grid[spawnPosition.x, spawnPosition.y] == null ||
                    gridManager.grid[spawnPosition.x, spawnPosition.y].layer == LayerMask.NameToLayer(gridManager.occupiedTileLayerName) || 
                    gridManager.nodeGrid[spawnPosition.x, spawnPosition.y].hasPlayer
                );

        Debug.Log(spawnPosition);
        return spawnPosition;
    }

    public void CheckAllEnemiesDefeated()
    {
        if (spawnedEnemies.Count <= 0)
        {
            OnAllEnemiesDefeated?.Invoke(true);
        }
    }

}
