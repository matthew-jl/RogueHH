using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum EnemyState
{
    Idle,
    Alert,
    Aggro,
    ReadyToAttack
}

public class EnemyController : MonoBehaviour, ITurnAction
{
    public CameraFollow cameraFollow;
    public EnemyManager enemyManager;
    public Enemy enemyData;

    private PlayerData playerData;
    private PlayerController player;
    private Transform playerTransform;

    public Transform enemyModel;
    private Pathfinding pathfinding;
    private List<Node> currentPath;
    private int pathIndex = 0;
    private bool isMoving = false;
    private float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    private GridManager gridManager;
    private Vector3 previousPosition;
    public string tileLayerName = "Tiles";
    public string occupiedTileLayerName = "OccupiedTiles";

    [Header("UI Elements")]
    public Slider hpBar; 
    public TMP_Text enemyNameText;
    public TMP_Text alertText;
    public TMP_Text damageText;
    public TMP_Text critDamageText;
    public TMP_Text aggroText;

    public EnemyState currentState = EnemyState.Idle;

    private bool actionComplete = false;

    public Animator enemyAnimator;

    public AudioSource walkingAudioSource;
    public AudioSource attackAudioSource;
    public AudioClip walkingSound;
    public AudioClip attackSwordSound;
    public AudioClip attackUnarmedSound;
    public AudioClip deathGruntSound;

    private void Start()
    {
        if (hpBar != null)
        {
            hpBar.maxValue = enemyData.health; 
            hpBar.value = enemyData.health;
        }
        if (enemyNameText != null)
        {
            enemyNameText.text = gameObject.name;
        }
        alertText.alpha = 0f;
        damageText.alpha = 0f;
        critDamageText.alpha = 0f;
        aggroText.alpha = 0f;

        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
        }

        cameraFollow = FindObjectOfType<CameraFollow>();

        pathfinding = FindObjectOfType<Pathfinding>(); 
        player = FindObjectOfType<PlayerController>();
        playerTransform = FindObjectOfType<PlayerController>().transform;
        gridManager = FindObjectOfType<GridManager>();

        previousPosition = transform.position;
        MarkTileOccupied(transform.position);
    }

    void Update()
    {
        enemyAnimator.SetBool("IsMoving", isMoving);

        if (IsPlayerInAlertRange())
        {
            if (currentState == EnemyState.Idle)
            {
                SetAlertState();
            } 
            else if (currentState == EnemyState.Aggro && IsPlayerInAttackRange())
            {
                SetReadyToAttackState();
            }
        }
        else
        {
            if (currentState == EnemyState.Alert)
            {
                SetIdleState();
            }
        }

        if (isMoving && !walkingAudioSource.isPlaying)
        {
            walkingAudioSource.clip = walkingSound;
            walkingAudioSource.loop = true;      
            walkingAudioSource.Play();
        }
        if (!isMoving && walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Stop();
            walkingAudioSource.loop = false;            
        }

    }

    public void Initialize(Enemy enemy)
    {
        enemyData = enemy;
    }

    void SetAlertState()
    {
        currentState = EnemyState.Alert;
        alertText.alpha = 1f;
        aggroText.alpha = 0f;
    }

    void SetIdleState()
    {
        currentState = EnemyState.Idle;
        alertText.alpha = 0f;
        aggroText.alpha = 0f;
    }

    void SetAggroState()
    {
        currentState = EnemyState.Aggro;
        alertText.alpha = 0f;
        aggroText.alpha = 1f;

        // rotate enemy to player
        Vector3 direction = playerTransform.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    void SetReadyToAttackState()
    {
        currentState = EnemyState.ReadyToAttack;
    }

    IEnumerator MoveEnemy()
    {
        while (true)
        {
            if (IsPlayerInAlertRange() || currentState == EnemyState.Aggro)
            {
                Vector3 playerPosition = playerTransform.position;
                List<Node> path = pathfinding.FindPath(transform.position, playerPosition);

                if (path != null && path.Count > 0)
                {
                    currentPath = path;
                    pathIndex = 0;
                    isMoving = true;
                    yield return StartCoroutine(FollowPath());

                    if (actionComplete)
                    {
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator FollowPath()
    {
        while (isMoving && currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[pathIndex].worldPosition + new Vector3(0, 0.5f, 0);

            // so the enemy cannot go to a node that has the player or another enemy
            if (currentPath[pathIndex].hasPlayer || currentPath[pathIndex].hasEnemy)
            {
                isMoving = false;
                currentPath = null;
                actionComplete = true;
                yield break;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            RotateEnemy(targetPosition);

            if (transform.position == targetPosition)
            {
                MarkTileOccupied(transform.position);
                UnmarkTileOccupied(previousPosition);
                previousPosition = transform.position;

                pathIndex++;
                if (pathIndex >= currentPath.Count || pathIndex >= 1)
                {
                    isMoving = false;  // Stop moving if the path is complete
                    currentPath = null;
                    actionComplete = true;
                    yield break;
                }
            }
            yield return null;
        }
    }

    public bool IsPlayerInAlertRange()
    {
        // calculate Manhattan distance to the player (alert range)
        int alertRange = 4; // The enemy's alert range in tiles

        int distance = Mathf.Abs(Mathf.RoundToInt(transform.position.x) - Mathf.RoundToInt(playerTransform.position.x)) +
                       Mathf.Abs(Mathf.RoundToInt(transform.position.z) - Mathf.RoundToInt(playerTransform.position.z));

        return distance <= alertRange;
    }

    bool IsPlayerInAttackRange()
    {
        // calculate Manhattan distance between the player and the enemy
        int distance = Mathf.Abs(Mathf.RoundToInt(transform.position.x) - Mathf.RoundToInt(playerTransform.position.x)) +
                       Mathf.Abs(Mathf.RoundToInt(transform.position.z) - Mathf.RoundToInt(playerTransform.position.z));

        return distance == 1; // 1 tile away in horizontal or vertical direction
    }

    public void ApplyDamage(int damage)
    {
        enemyData.health -= damage;

        if (hpBar != null)
        {
            hpBar.value = enemyData.health;
        }

        if (enemyData.health <= 0)
        {
            hpBar.fillRect.gameObject.SetActive(false);
            Die();
        }
    }

    public void showDamageText(int damage)
    {
        if (enemyData.health > 0)
        {
            damageText.text = damage.ToString();
            StartCoroutine(FadeInAndOut(damageText));
        }
    }

    public void showCritDamageText(int damage)
    {
        if (enemyData.health > 0)
        {
            critDamageText.text = damage.ToString();
            StartCoroutine(FadeInAndOut(critDamageText));
        }
        cameraFollow.TriggerScreenShake();
    }

    private IEnumerator FadeInAndOut(TMP_Text text)
    {
        float fadeDuration = 0.5f;

        text.alpha = 0f;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            text.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration); 
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        text.alpha = 1f;

        timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            text.alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        text.alpha = 0f; 
    }

    void RotateEnemy(Vector3 targetPosition)
    {
        // calculate the direction from the enemy to the target position
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f; // to ensure we only rotate on the XZ plane

        if (direction.magnitude > 0.1f)
        {
            // calculate the target rotation angle (in the Y axis)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // smoothly rotate the enemy model to face the target angle
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            enemyModel.rotation = Quaternion.Slerp(enemyModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void MarkTileOccupied(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.tileSpacing);
        int z = Mathf.RoundToInt(position.z / gridManager.tileSpacing);

        if (x >= 0 && x < gridManager.gridWidth && z >= 0 && z < gridManager.gridHeight)
        {
            gridManager.occupiedCells[x, z] = true;
            gridManager.grid[x, z].layer = LayerMask.NameToLayer(occupiedTileLayerName);
            gridManager.nodeGrid[x, z].hasEnemy = true;
        }
    }

    private void UnmarkTileOccupied(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.tileSpacing);
        int z = Mathf.RoundToInt(position.z / gridManager.tileSpacing);

        if (x >= 0 && x < gridManager.gridWidth && z >= 0 && z < gridManager.gridHeight)
        {
            gridManager.occupiedCells[x, z] = false;
            gridManager.grid[x, z].layer = LayerMask.NameToLayer(tileLayerName);
            gridManager.nodeGrid[x, z].hasEnemy = false;
        }
    }

    void Die()
    {
        enemyAnimator.SetTrigger("Die");
        attackAudioSource.PlayOneShot(deathGruntSound);
        StartCoroutine(WaitForDeathAnimation());
    }

    IEnumerator WaitForDeathAnimation()
    {
        AnimatorStateInfo stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);

        float animationLength = stateInfo.length;

        yield return new WaitForSeconds(animationLength);

        UnmarkTileOccupied(transform.position);

        if (enemyManager != null)
        {
            if (enemyManager.spawnedEnemies.Contains(this))
            {
                enemyManager.spawnedEnemies.Remove(this);
                enemyManager.CheckAllEnemiesDefeated();
            }
        }

        GrantRewards();

        gameObject.SetActive(false);
    }

    void GrantRewards()
    {
        int baseEXP = 50;
        int baseZhen = 5;

        int expReward = baseEXP * (playerData.currentFloor);
        int zhenReward = baseZhen * (playerData.currentFloor);

        switch (enemyData.type)
        {
            case EnemyType.Common:
                break;
            case EnemyType.Medium:
                expReward *= 2;
                zhenReward *= 2;
                break;
            case EnemyType.Elite:
                expReward *= 3;
                zhenReward *= 5;
                break;
        }

        playerData.AddExperience(expReward);
        playerData.AddZhen(zhenReward);

        Debug.Log($"Gained {expReward} EXP and {zhenReward} Zhen");
    }

    bool CheckLOS()
    {
        RaycastHit hit;
        Debug.Log("Enemy: " + transform.position);
        Debug.Log("Player: " + playerTransform.position);
        Vector3 direction = playerTransform.position - transform.position;
        Debug.Log("Direction: " + direction);

        Vector3 raycastSource = transform.position + new Vector3(0, 0.1f, 0);

        Debug.DrawRay(raycastSource, direction, Color.green, 2f);
        if (Physics.Raycast(raycastSource, direction, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("LOS VALID");
                return true; // LOS is valid
            }
        }
        Debug.Log("LOS INVALID");
        return false; // LOS blocked by obstacles
    }

    void PerformAttack()
    {
        Vector3 direction = playerTransform.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

        int damage = CalculateDamage();
        playerData.ApplyDamage(damage);

        player.showDamageText(damage);

        enemyAnimator.SetTrigger("Attack");

        if (enemyData.type == EnemyType.Common)
        {
            attackAudioSource.PlayOneShot(attackUnarmedSound);
        }
        else
        {
            attackAudioSource.PlayOneShot(attackSwordSound);
        }

        StartCoroutine(WaitAttackAnimation());
    }

    int CalculateDamage()
    {
        int playerDefense = playerData.CalculateCurrentDefense();

        float defenseScalingFactor = 50f;
        float defenseFactor = 1 - (playerDefense / (playerDefense + defenseScalingFactor));

        float finalDamage = enemyData.attack * defenseFactor;
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage)); // ensure a minimum of 1 damage
    }

    IEnumerator WaitAttackAnimation()
    {
        yield return new WaitForSeconds(enemyAnimator.GetCurrentAnimatorStateInfo(0).length - 1);
        actionComplete = true;
    }

    public void ExecuteAction()
    {
        // if enemy is already dead, skip the turn
        if (!gameObject.activeSelf)
        {
            actionComplete = true;
            return;
        }

        if (currentState == EnemyState.Idle)
        {
            actionComplete = true;
            return;
        }

        // if enemy is alert, check LOS, if LOS is valid, set state to aggro
        if (currentState == EnemyState.Alert)
        {
            if (CheckLOS())
            {
                SetAggroState();
            }
            actionComplete = true;
            return;
        }

        if (currentState == EnemyState.Aggro)
        {
            StartCoroutine(MoveEnemy());
            return;
        }

        if (currentState == EnemyState.ReadyToAttack)
        {
            if (IsPlayerInAttackRange())
            {
                PerformAttack();
            } 
            else
            {
                StartCoroutine(MoveEnemy());
            }
            return;
        }
    }

    public bool IsActionComplete()
    {
        return actionComplete;
    }

    public void ResetActionComplete()
    {
        actionComplete = false;
    }
}
