using UnityEngine;
using System.Collections;

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

    private GameObject currentChargingIndicator; 
    private Vector2Int gridPosition;
    private Vector2Int facingDirection;
    public bool isCharging = false;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridPosition = GridManager.Instance.WorldToGridPosition(transform.position);
        facingDirection = initialFacingDirection; 
        currentHealth = maxHealth;
        UpdatePosition();
        UpdateFacingDirection();
    }

    public void PerformTurn()
    {
        Vector2Int playerPosition = FindPlayerPosition();

        if (isCharging)
        {
            Attack();
        }
        else if (!IsFacingPlayer(playerPosition))
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

    private void Attack()//敌人攻击
    {
        isCharging = false;
        RemoveChargingIndicator(); // 移除蓄力指示器

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && IsPlayerInAttackRange(player.GetGridPosition()))
        {
            int damage = type == EnemyType.Sword ? swordDamage : spearDamage;
            Debug.Log($"Enemy attacked player for {damage} damage!");
            animator.SetTrigger("Attack");
            player.TakeDamage(damage);
        }
        else
        {
            Debug.Log("Player is not in attack range. Attack missed!");
        }
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
        if (facingDirection.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (facingDirection.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth <= 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died!");
        RemoveChargingIndicator();
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Empty);

        // 播放死亡动画
        StartCoroutine(PlayDeathAnimation());
    }

    private IEnumerator PlayDeathAnimation()
    {
        // 禁用敌人的其他行为（如果有的话）

        if (animator != null)
        {
            animator.SetTrigger("Die");

            // 等待动画播放完毕
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
        SetPosition(newPosition);
        facingDirection = newFacingDirection;
        UpdateFacingDirection();
    }
}