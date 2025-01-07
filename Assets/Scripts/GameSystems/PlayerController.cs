using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using static UnityEngine.ParticleSystem;

public class PlayerController : MonoBehaviour, ITurnAction
{
    public PlayerData playerData;

    public Pathfinding pathfinding;
    private List<Node> currentPath;
    private int pathIndex = 0;

    public GridManager gridManager; 

    // to highlight tiles when hover
    public Material highlightMaterial;
    private List<GameObject> highlightedTiles = new List<GameObject>();
    private Material originalMaterial;
    private Vector3 lastHoverTilePosition = Vector3.zero;

    public string tileLayerName = "Tiles";
    public string occupiedTileLayerName = "OccupiedTiles";
    private Vector3 previousPosition;

    public Animator playerAnimator;
    private bool isMoving = false;
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;
    public Transform playerModel;

    public EnemyManager enemyManager;
    private bool isInAlertRange = false;

    public UnityEvent<bool> OnPlayerDeath;
    public UnityEvent<bool> OnPlayerTurn;
    [SerializeField] private bool actionComplete = false;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private bool playerCanMove;

    public GameObject sword;
    public GameObject swordTrail;

    public TMP_Text damageText;
    public TMP_Text levelUpText;

    public SkillManager skillManager;
    public LifestealBuffSkill lifestealSkill;
    public GameObject lifestealEffect;
    private bool isLifestealActivated = false;
    public BashActiveSkill bashSkill;
    private bool isBashActivated = false;
    public ParticleSystem swordTrailParticleSystem;

    public AudioSource walkingAudioSource;
    public AudioSource attackAudioSource;
    public AudioClip walkingSound;
    public AudioClip attackSwordSound;
    public AudioClip deathGruntSound;

    // subscribe to events
    void OnEnable()
    {
        FindObjectOfType<PauseMenu>().OnPause.AddListener(HandlePauseStateChange);
        FindObjectOfType<EnemyManager>().OnAllEnemiesDefeated.AddListener(HandleAllEnemiesDefeated);
        playerData.OnLevelUp.AddListener(HandleLevelUp);
        lifestealSkill.OnLifestealActivated.AddListener(HandleLifestealActivated);
        bashSkill.OnBashActivated.AddListener(HandleBashActivated);
    }

    // unsub from events
    void OnDisable()
    {
        if (FindObjectOfType<PauseMenu>())
        {
            FindObjectOfType<PauseMenu>().OnPause.RemoveListener(HandlePauseStateChange);
        }
        if (FindObjectOfType<EnemyManager>())
        {
            FindObjectOfType<EnemyManager>().OnAllEnemiesDefeated.RemoveListener(HandleAllEnemiesDefeated);
        }
        if (playerData)
        {
            playerData.OnLevelUp.RemoveListener(HandleLevelUp);
        }
        lifestealSkill.OnLifestealActivated.RemoveListener(HandleLifestealActivated);
        bashSkill.OnBashActivated.RemoveListener(HandleBashActivated);
    }

    void HandleBashActivated(bool isBashActivated)
    {
        this.isBashActivated = isBashActivated;
    }

    void HandleLifestealActivated(bool isLifestealActivated)
    {
        this.isLifestealActivated = isLifestealActivated;
        if (isLifestealActivated)
        {
            lifestealEffect.SetActive(true);
            playerAnimator.SetTrigger("ActivateSkill");
        }
        else
        {
            lifestealEffect.SetActive(false);
        }
    }

    void ApplyLifestealDuringAttack(float damageDealt)
    {
        if (isLifestealActivated)
        {
            lifestealSkill.ApplyLifesteal(damageDealt, playerData);
        }
    }

    void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager reference is missing.");
        }
        if (pathfinding == null)
        {
            Debug.LogError("Pathfinding reference is missing.");
        }
        if (enemyManager == null)
        {
            Debug.LogError("EnemyManager reference is missing.");
        }
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.playerData;
        }

        previousPosition = transform.position;
        MarkTileOccupied(transform.position);

        sword.SetActive(false);
        swordTrail.SetActive(false);
        lifestealEffect.SetActive(false);

        damageText.alpha = 0f;
        levelUpText.alpha = 0f;
    }
    void Update()
    {
        isInAlertRange = IsPlayerInEnemyAlertRange();

        if (!isMoving && !isPaused)
        {
            if (Input.GetKeyUp("space")) {
                OnPlayerTurn?.Invoke(false);
                actionComplete = true;
            }
            HandleHover();
        }

        playerAnimator.SetBool("IsMoving", isMoving);

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

        if (Input.GetMouseButtonDown(0) && !isPaused && playerCanMove)
        {
            if (IsPointerOverEnemy())
            {
                EnemyController clickedEnemy = GetClickedEnemy();

                if (clickedEnemy != null && IsEnemyInAttackRange(clickedEnemy))
                {
                    PerformAttack(clickedEnemy);
                }
                return;
            }

            if (isMoving)
            {
                if (IsPointerOverTopFace())
                {
                    isMoving = false;
                    currentPath = null;
                    ClearHighlightedTiles();
                }
            }
            else
            {
                if (IsPointerOverTopFace())
                {
                    if (currentPath != null && currentPath.Count > 0)
                    {
                        isMoving = true;
                        ClearHighlightedTiles();
                        StopCoroutine("FollowPath");
                        StartCoroutine("FollowPath");
                    }
                }
            }
        }

    }

    void HandleHover()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        int layerMask = LayerMask.GetMask(tileLayerName);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.9f)
            {
                Vector3 hoverTilePosition = hit.collider.gameObject.transform.position;

                if (hoverTilePosition != lastHoverTilePosition)
                {
                    lastHoverTilePosition = hoverTilePosition;

                    List<Node> path;
                    path = pathfinding.FindPath(transform.position, hoverTilePosition);

                    if (path != null)
                    {
                        currentPath = path;
                        HighlightPath(path);
                    }
                    else
                    {
                        ClearHighlightedTiles();
                    }
                }
                else
                {
                    // even if the position hasn't changed, ensure the path remains highlighted
                    if (currentPath != null)
                    {
                        HighlightPath(currentPath);
                    }
                }
            }
            else
            {
                ClearHighlightedTiles();
                lastHoverTilePosition = Vector3.zero;
            }
        }
        else
        {
            ClearHighlightedTiles();
            lastHoverTilePosition = Vector3.zero;
        }
    }

    void HighlightPath(List<Node> path)
    {
        ClearHighlightedTiles();

        foreach (Node node in path)
        {
            int x = node.gridX;
            int z = node.gridZ;
            GameObject tile = gridManager.grid[x, z];

            if (tile != null)
            {
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (originalMaterial == null)
                    {
                        originalMaterial = renderer.material;
                    }

                    renderer.material = highlightMaterial;

                    highlightedTiles.Add(tile);
                }
            }
        }
    }

    void ClearHighlightedTiles()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            if (tile != null)
            {
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null && originalMaterial != null)
                {
                    renderer.material = originalMaterial;
                }
            }
        }
        highlightedTiles.Clear();
    }

    IEnumerator FollowPath()
    {
        actionComplete = false;
        OnPlayerTurn?.Invoke(true);
        pathIndex = 0;

        if (currentPath == null || currentPath.Count == 0 || currentPath[0].hasEnemy)
        {
            isMoving = false;
            yield break;
        }

        Vector3 currentWaypoint = currentPath[0].worldPosition + new Vector3(0, 0.5f, 0);

        while (true)
        {

            // only execute this code after the player moved one tile (to the center of tile)
            if (transform.position == currentWaypoint)
            {
                if (isLifestealActivated)
                {
                    lifestealSkill.DecreaseRemainingTurnsByOne();
                }

                if (!isMoving || currentPath == null)
                {
                    OnPlayerTurn?.Invoke(false);
                    actionComplete = true;
                    yield break;
                }

                // mark current tile unwalkable and unmark previous tile
                MarkTileOccupied(transform.position);
                UnmarkTileOccupied(previousPosition);
                previousPosition = transform.position;

                pathIndex++;
                // if reach end of path, or if the player is in alert range, stop moving
                if (pathIndex >= currentPath.Count || isInAlertRange || currentPath[pathIndex].hasEnemy)
                {
                    OnPlayerTurn?.Invoke(false);
                    actionComplete = true;

                    currentPath = null;
                    isMoving = false;
                    yield break;
                }
                currentWaypoint = currentPath[pathIndex].worldPosition + new Vector3(0, 0.5f, 0);
            }

            RotatePlayer(currentWaypoint);

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, moveSpeed * Time.deltaTime);

            yield return null;
        }
    }

    void RotatePlayer(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f; // to ensure we only rotate on the XZ plane

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // if we want to rotate the player model to face the target angle immediately:
            // playerModel.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
    }

    bool IsPointerOverTopFace()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        int layerMask = LayerMask.GetMask(tileLayerName);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            // check if the hit normal is pointing upwards (top face)
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.9f)
            {
                return true;
            }
        }
        return false;
    }

    private void MarkTileOccupied(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.tileSpacing);
        int z = Mathf.RoundToInt(position.z / gridManager.tileSpacing);

        if (x >= 0 && x < gridManager.gridWidth && z >= 0 && z < gridManager.gridHeight)
        {
            gridManager.occupiedCells[x, z] = true;
            gridManager.grid[x, z].layer = LayerMask.NameToLayer(occupiedTileLayerName);
            gridManager.nodeGrid[x, z].hasPlayer = true;
        }
    }

    // Method to unmark the previous tile as unoccupied
    private void UnmarkTileOccupied(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.tileSpacing);
        int z = Mathf.RoundToInt(position.z / gridManager.tileSpacing);

        if (x >= 0 && x < gridManager.gridWidth && z >= 0 && z < gridManager.gridHeight)
        {
            gridManager.occupiedCells[x, z] = false;
            gridManager.grid[x, z].layer = LayerMask.NameToLayer(tileLayerName);
            gridManager.nodeGrid[x, z].hasPlayer = false;
        }
    }

    bool IsPlayerInEnemyAlertRange()
    {
        if (enemyManager == null)
        {
            Debug.LogError("EnemyManager reference is missing.");
            return false;
        }

        foreach (EnemyController enemyController in enemyManager.spawnedEnemies)
        {
            Vector3 enemyPosition = enemyController.transform.position;
            Vector3 playerPosition = transform.position;

            int enemyX = Mathf.RoundToInt(enemyPosition.x / gridManager.tileSpacing);
            int enemyZ = Mathf.RoundToInt(enemyPosition.z / gridManager.tileSpacing);
            int playerX = Mathf.RoundToInt(playerPosition.x / gridManager.tileSpacing);
            int playerZ = Mathf.RoundToInt(playerPosition.z / gridManager.tileSpacing);

            // calculate the Manhattan distance (diamond-shaped area)
            int distance = Mathf.Abs(enemyX - playerX) + Mathf.Abs(enemyZ - playerZ);

            int alertRange = 5; // the enemy's alert range in tiles

            if (distance <= alertRange)
            {
                return true;
            }
        }

        return false;
    }

    bool IsPointerOverEnemy()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy")) 
            {
                return true;
            }
        }

        return false;
    }

    EnemyController GetClickedEnemy()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                return hit.collider.GetComponent<EnemyController>();
            }
        }

        return null;
    }

    bool IsEnemyInAttackRange(EnemyController enemy)
    {
        int playerX = Mathf.RoundToInt(transform.position.x / gridManager.tileSpacing);
        int playerZ = Mathf.RoundToInt(transform.position.z / gridManager.tileSpacing);
        int enemyX = Mathf.RoundToInt(enemy.transform.position.x / gridManager.tileSpacing);
        int enemyZ = Mathf.RoundToInt(enemy.transform.position.z / gridManager.tileSpacing);

        int distance = Mathf.Abs(playerX - enemyX) + Mathf.Abs(playerZ - enemyZ);

        return distance == 1;
    }

    void PerformAttack(EnemyController enemy)
    {
        // rotate player to enemy
        Vector3 direction = enemy.enemyModel.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        playerModel.rotation = Quaternion.Euler(0f, targetAngle, 0f);

        // deal damage to the enemy
        bool isCritical = IsCriticalHit();
        float critDamage = playerData.CalculateCurrentCritDamage();
        int playerBaseAttack = playerData.CalculateCurrentAttack();
        int damage = CalculateDamage(playerBaseAttack, enemy.enemyData.defense, enemy.enemyData.defenseScalingFactor, isCritical, critDamage);

        // bash
        if (isBashActivated)
        {
            damage = Mathf.FloorToInt(damage * 1.5f);
            // change sword trail
            if (swordTrailParticleSystem != null)
            {
                var trailModule = swordTrailParticleSystem.trails;
                MinMaxGradient bashGradient = new MinMaxGradient(Color.yellow, new Color(1f, 0.5f, 0f));
                trailModule.colorOverLifetime = bashGradient;
            }
            // turn off bash
            skillManager.ActivateSkill(1);
        }

        enemy.ApplyDamage(damage);
        Debug.Log("Dealt damage: " + damage + " (critical: " + isCritical + ")");

        // lifesteal
        ApplyLifestealDuringAttack(damage);
        if (isLifestealActivated)
        {
            lifestealSkill.DecreaseRemainingTurnsByOne();
        }

        // damage text
        if (isCritical) enemy.showCritDamageText(damage);
        else enemy.showDamageText(damage);

        sword.SetActive(true);
        swordTrail.SetActive(true);

        PlayRandomAttackAnimation();

        attackAudioSource.PlayOneShot(attackSwordSound);

        StartCoroutine(DisableSwordAfterAnimation());
    }

    bool IsCriticalHit()
    {
        // if crit rate is greater than a random value, it's a critical hit
        float critRate = playerData.CalculateCurrentCritRate();
        return Random.Range(0f, 100f) < critRate;
    }

    public int CalculateDamage(int playerAttack, int enemyDefense, int defenseScalingFactor, bool isCritical, float critDamage)
    {
        float defenseFactor = 1f - (enemyDefense / (float)(enemyDefense + defenseScalingFactor));

        float damageOutput;
        if (isCritical)
        {
            damageOutput = playerAttack * (critDamage * 0.01f) * defenseFactor;
        } 
        else
        {
            damageOutput = playerAttack * defenseFactor;
        }

        // slight randomization (±5% of the damage)
        float randomFactor = Random.Range(0.95f, 1.05f);
        damageOutput *= randomFactor;

        return Mathf.RoundToInt(damageOutput);
    }

    void PlayRandomAttackAnimation()
    {
        int randomAttack = Random.Range(1, 4);

        switch (randomAttack)
        {
            case 1:
                playerAnimator.SetTrigger("AttackVariation1");
                break;
            case 2:
                playerAnimator.SetTrigger("AttackVariation2");
                break;
            case 3:
                playerAnimator.SetTrigger("AttackVariation3");
                break;
        }
    }

    IEnumerator DisableSwordAfterAnimation()
    {
        yield return new WaitForSeconds(playerAnimator.GetCurrentAnimatorStateInfo(0).length - 1);

        sword.SetActive(false);
        swordTrail.SetActive(false);

        if (swordTrailParticleSystem != null)
        {
            var trailModule = swordTrailParticleSystem.trails;

            MinMaxGradient defaultGradient = new MinMaxGradient(Color.white);
            trailModule.colorOverLifetime = defaultGradient;
        }

        // finish turn after attacking
        OnPlayerTurn?.Invoke(false);
        actionComplete = true;
    }

    public void showDamageText(int damage)
    {
        if (playerData.currentHealth > 0)
        {
            damageText.text = damage.ToString();
            StartCoroutine(FadeInAndOut(damageText));
        } 
        else
        {
            Die();
        }
    }

    private void ShowLevelUpText()
    {
        StartCoroutine(FadeInAndOut(levelUpText));
    }

    private IEnumerator FadeInAndOut(TMP_Text text)
    {
        float fadeDuration = 0.5f;

        text.alpha = 0f;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            text.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration); // fade in
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        text.alpha = 1f;

        // if we want a delay:
        // yield return new WaitForSeconds(0.5f);

        timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            text.alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration); // fade out
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        text.alpha = 0f;
    }

    void Die()
    {
        playerAnimator.SetTrigger("Die");
        attackAudioSource.PlayOneShot(deathGruntSound);
        StartCoroutine(WaitForDeathAnimation());
    }

    IEnumerator WaitForDeathAnimation()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);
        UnmarkTileOccupied(transform.position);
        gameObject.SetActive(false);
        OnPlayerDeath?.Invoke(true);
    }

    public void HandlePauseStateChange(bool isPaused)
    {
        this.isPaused = isPaused;
    }

    private void HandleLevelUp()
    {
        ShowLevelUpText();
    }

    private void HandleAllEnemiesDefeated(bool onAllEnemiesDefeated)
    {
        if (onAllEnemiesDefeated)
        {
            gameObject.SetActive(false);
        }
    }

    void ITurnAction.ExecuteAction()
    {
        playerCanMove = true;
        return;
    }

    bool ITurnAction.IsActionComplete()
    {
        return actionComplete;
    }

    public void ResetActionComplete()
    {
        playerCanMove = false;
        actionComplete = false;
    }
}
