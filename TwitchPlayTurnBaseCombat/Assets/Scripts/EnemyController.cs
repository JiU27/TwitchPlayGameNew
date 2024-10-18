using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Sword, Spear }

    [SerializeField] private EnemyType type;
    [SerializeField] private int swordDamage = 5;
    [SerializeField] private int spearDamage = 3;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private Vector2Int initialFacingDirection = Vector2Int.left; 
    [SerializeField] private Animator animator; // Attack功能, PlayDeathAnimation功能都需要动画
    [SerializeField] private float deathAnimationDuration = 0.5f; // 死亡动画的持续时间
    [SerializeField] private GameObject chargingIndicatorPrefab; 
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 1, 0);
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Slider healthBar;

    public GameObject particleEffectPrefab;
    public Transform spawnPosition;

    private GameObject currentChargingIndicator; 
    private Vector2Int gridPosition;
    private Vector2Int facingDirection;
    public bool isCharging = false;
    private int currentHealth;

    public AudioSource enemy_hurt;

    //[SerializeField] private Sprite normalSprite;
    //[SerializeField] private Sprite chargingSprite;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float switchAnimationDuration = 0.5f;

    private void Start()
    {
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on enemy or its children!");
        }
        SetNormalSprite();

        gridPosition = GridManager.Instance.WorldToGridPosition(transform.position);
        facingDirection = initialFacingDirection; 
        currentHealth = maxHealth;
        UpdatePosition();
        UpdateFacingDirection();
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<Slider>();
        }

        if (healthBar != null)
        {
            healthBar.maxValue = 1f;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("Health bar not found for enemy: " + gameObject.name);
        }
    }
    public void PerformTurn()
    {
        if (isCharging)
        {
            Attack();
        }
        else if(CheckLineOfSight())
        {  
            Vector2Int playerPosition = FindPlayerPosition();

            if (!IsFacingPlayer(playerPosition))
            {
                TurnTowardsPlayer(playerPosition);
            }
            else if (IsPlayerInAttackRange(playerPosition))
            {
                StartCharging();
            }
            else
            {
                MoveTowardsPlayer(playerPosition);
            }

        }
        else
        {
            Wait();
        }
    }

    private bool CheckLineOfSight()
    {
        Vector2Int checkPosition = gridPosition;
        Vector2Int playerPosition = FindPlayerPosition();

        while (GridManager.Instance.IsValidGridPosition(checkPosition))
        {
            checkPosition += facingDirection;
            GridManager.CellType cellType = GridManager.Instance.GetCellType(checkPosition);

            if (cellType == GridManager.CellType.Player)
            {
                return true; // 视线中第一个遇到的是玩家
            }
            else if (cellType == GridManager.CellType.Enemy || cellType == GridManager.CellType.Obstacle)
            {
                // 如果视线被阻挡，尝试转向
                TurnTowardsPlayer(playerPosition);
                return false; // 视线被其他敌人或障碍物阻挡
            }
        }
        // 如果没有检测到任何东西，也尝试转向
        TurnTowardsPlayer(playerPosition);
        return false;
    }

    private void Wait()
    {
        Debug.Log("Enemy is waiting this turn.");
        // 可以在这里添加等待动画或其他视觉反馈
    }

    private Vector2Int FindPlayerPosition()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        return player != null ? player.GetGridPosition() : Vector2Int.zero;
    }

    private bool IsFacingPlayer(Vector2Int playerPosition)
    {
        Vector2Int directionToPlayer = playerPosition - gridPosition;

        if (directionToPlayer.x != 0) directionToPlayer.x = directionToPlayer.x / Mathf.Abs(directionToPlayer.x);
        if (directionToPlayer.y != 0) directionToPlayer.y = directionToPlayer.y / Mathf.Abs(directionToPlayer.y);

        return directionToPlayer == facingDirection;
    }

    private void TurnTowardsPlayer(Vector2Int playerPosition)
    {
        Vector2Int directionToPlayer = playerPosition - gridPosition;

        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.y))
        {
            facingDirection = directionToPlayer.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            facingDirection = directionToPlayer.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        UpdateFacingDirection();
        Debug.Log("Enemy turned towards player");
    }

    private bool IsPlayerInAttackRange(Vector2Int playerPosition)
    {
        Vector2Int attackPosition = gridPosition + facingDirection;

        switch (type)
        {
            case EnemyType.Sword:
                return playerPosition == attackPosition;
            case EnemyType.Spear:
                return playerPosition == attackPosition || playerPosition == (gridPosition + facingDirection * 2);
            default:
                return false;
        }
    }

    private void StartCharging()
    {
        SetChargingSprite();
        isCharging = true;
        Debug.Log("Enemy started charging");
        CreateChargingIndicator();
    }

    private void CreateChargingIndicator()
    {
        if (chargingIndicatorPrefab != null)
        {
            Vector3 indicatorPosition = transform.position + indicatorOffset;
            currentChargingIndicator = Instantiate(chargingIndicatorPrefab, indicatorPosition, Quaternion.identity, transform);
        }
        else
        {
            Debug.LogWarning("Charging indicator prefab is not assigned!");
        }
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
        isCharging = false;
        RemoveChargingIndicator();
        SetNormalSprite();

        Vector2Int attackPosition = gridPosition + facingDirection;
        GridManager.CellType targetCellType = GridManager.Instance.GetCellType(attackPosition);

        if (targetCellType == GridManager.CellType.Player)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && player.GetGridPosition() == attackPosition)
            {
                int damage = type == EnemyType.Sword ? swordDamage : spearDamage;
                Debug.Log($"Enemy attacked player for {damage} damage!");
                player.TakeDamage(damage);
            }
        }
        else if (targetCellType == GridManager.CellType.Enemy)
        {
            EnemyController targetEnemy = FindEnemyAt(attackPosition);
            if (targetEnemy != null)
            {
                int damage = type == EnemyType.Sword ? swordDamage : spearDamage;
                Debug.Log($"Enemy attacked another enemy for {damage} damage!");
                // 修改这里，传入 false 作为 playerFacingRight 参数
                // 因为这是敌人攻击敌人，所以我们不需要考虑玩家朝向
                targetEnemy.TakeDamage(damage, false);
            }
        }
        else
        {
            Debug.Log("Attack missed! No valid target in range.");
        }
    }

    private EnemyController FindEnemyAt(Vector2Int position)
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            if (enemy != this && enemy.GetGridPosition() == position)
            {
                return enemy;
            }
        }
        return null;
    }

    private void RemoveChargingIndicator()
    {
        if (currentChargingIndicator != null)
        {
            Destroy(currentChargingIndicator);
            currentChargingIndicator = null;
        }
    }

    private bool IsPlayerInAttackRange(Vector2Int playerPosition, Vector2Int? direction = null)
    {
        Vector2Int checkDirection = direction ?? (playerPosition - gridPosition);
        int distance = Mathf.Abs(checkDirection.x) + Mathf.Abs(checkDirection.y);

        if (type == EnemyType.Sword)
        {
            return distance <= 1 && checkDirection == facingDirection;
        }
        else // Spear
        {
            return distance <= 2 &&
                   (checkDirection == facingDirection || checkDirection == facingDirection * 2);
        }
    }

    private void MoveTowardsPlayer(Vector2Int playerPosition)
    {
        Vector2Int direction = playerPosition - gridPosition;
        Vector2Int moveDirection = Vector2Int.zero;

        if (direction.x != 0)
        {
            moveDirection.x = direction.x > 0 ? 1 : -1;
        }
        else if (direction.y != 0)
        {
            moveDirection.y = direction.y > 0 ? 1 : -1;
        }

        Vector2Int newPosition = gridPosition + moveDirection;

        if (GridManager.Instance.CanMoveTo(newPosition))
        {
            Vector2Int oldPosition = gridPosition;
            gridPosition = newPosition;
            GridManager.Instance.MoveEntity(oldPosition, gridPosition, GridManager.CellType.Enemy);
            StartCoroutine(MoveAnimation(GridManager.Instance.GridToWorldPosition(gridPosition)));
        }
    }

    private IEnumerator MoveAnimation(Vector3 targetPosition)
    {
        //animator.SetBool("IsMoving", true);
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        //animator.SetBool("IsMoving", false);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Enemy);
    }

    private void UpdateFacingDirection()
    {
        if (spriteTransform != null)
        {
            if (facingDirection.x < 0)
            {
                spriteTransform.localScale = new Vector3(-4, 4, 4);
            }
            else if (facingDirection.x > 0)
            {
                spriteTransform.localScale = new Vector3(4, 4, 4);
            }
        }
        else
        {
            Debug.LogWarning("Sprite transform reference is missing!");
        }
    }

    public void TakeDamage(int damage, bool playerFacingRight = false)
    {
        GameObject particleInstance = Instantiate(particleEffectPrefab, spawnPosition.position, spawnPosition.rotation);

        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        Destroy(particleInstance, particleSystem.main.duration);

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();


        enemy_hurt.Play();


        if (playerFacingRight)
        {
            EffectsManager.Instance.PlayEnemyDamageEffects(transform.position, playerFacingRight);
        }
        else
        {
            EffectsManager.Instance.PlayPlayerDamageEffects(transform.position);
        }

        if (currentHealth <= 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died!");
        RemoveChargingIndicator();
        SetNormalSprite();
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Empty);

        GameManager.Instance.RemoveEnemy(this);

        StartCoroutine(PlayDeathAnimation());
    }

    private IEnumerator PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
            yield return new WaitForSeconds(deathAnimationDuration);
        }
        else
        {
            Debug.LogWarning("Animator component is missing on the enemy!");
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(gameObject);
    }


    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public Vector2Int GetGridPosition() => gridPosition;
    public Vector2Int GetFacingDirection() => facingDirection;

    public void SetPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Enemy);
    }

    public void UpdatePositionAndDirection(Vector2Int newPosition, Vector2Int newFacingDirection)
    {
        StartCoroutine(SwitchPositionAnimation(newPosition, newFacingDirection));
        SetNormalSprite(); 
    }

    private IEnumerator SwitchPositionAnimation(Vector2Int newPosition, Vector2Int newFacingDirection)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = GridManager.Instance.GridToWorldPosition(newPosition);
        Vector3 midPoint = (startPosition + targetPosition) / 2;

        yield return StartCoroutine(MoveToPosition(midPoint));

        yield return StartCoroutine(MoveToPosition(targetPosition));

        SetPosition(newPosition);
        facingDirection = newFacingDirection;
        UpdateFacingDirection();
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < switchAnimationDuration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / switchAnimationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    private void SetNormalSprite()
    {
        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
    }

    private void SetChargingSprite()
    {
        if (animator != null)
        {
            animator.SetTrigger("Charge");
        }
    }
}