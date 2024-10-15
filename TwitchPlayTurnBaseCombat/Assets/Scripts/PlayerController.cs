using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int attackCooldown = 2;
    [SerializeField] private int switchCooldown = 3;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Vector2Int initialFacingDirection = Vector2Int.right;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Animator animator; // Attack功能, PlayDeathAnimation功能都需要动画
    [SerializeField] private GameOverPanel gameOverPanel;

    private Vector2Int gridPosition;
    private Vector2Int facingDirection;
    private int currentAttackCooldown = 0;
    private int currentSwitchCooldown = 0;
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

    public void PerformAction(string action)
    {
        Debug.Log($"Performing action: {action}");
        switch (action)
        {
            case "MoveLeft":
                Move(Vector2Int.left);
                break;
            case "MoveRight":
                Move(Vector2Int.right);
                break;
            case "Turn":
                Turn();
                break;
            case "Wait":
                Wait();
                break;
            case "Attack":
                Attack();
                break;
            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }

        DecreaseCooldowns();
    }

    private void Move(Vector2Int direction)
    {
        Debug.Log($"Move called. Direction: {direction}, Current position: {gridPosition}, Current facing: {facingDirection}");

        Vector2Int newPosition = gridPosition + direction;
        if (GridManager.Instance.CanMoveTo(newPosition))
        {
            Vector2Int oldPosition = gridPosition;
            gridPosition = newPosition;
            GridManager.Instance.MoveEntity(oldPosition, gridPosition, GridManager.CellType.Player);
            StartCoroutine(MoveAnimation(GridManager.Instance.GridToWorldPosition(gridPosition)));

            Debug.Log($"Moved to: {gridPosition}, Facing direction remains: {facingDirection}");
        }
        else if (GridManager.Instance.GetCellType(newPosition) == GridManager.CellType.Enemy)
        {
            if (currentSwitchCooldown <= 0)
            {
                SwitchWithEnemy(newPosition);
            }
            else
            {
                Debug.Log($"Switch on cooldown. {currentSwitchCooldown} turns remaining.");
            }
        }
        else
        {
            Debug.Log($"Cannot move to {newPosition}. Cell type: {GridManager.Instance.GetCellType(newPosition)}");
        }
    }

    private void Turn()
    {
        facingDirection = -facingDirection;
        UpdateFacingDirection(facingDirection);
        Debug.Log("Player turned around");
    }

    public void UpdateFacingDirection(Vector2Int newDirection)
    {
        if (newDirection != Vector2Int.zero)
        {
            facingDirection = newDirection;
            Debug.Log($"Facing direction updated to: {facingDirection}");
            UpdateSpriteRotation();
        }
    }

    private void UpdateSpriteRotation()
    {
        if (playerSprite != null)
        {
            if (facingDirection.x < 0)
            {
                playerSprite.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (facingDirection.x > 0)
            {
                playerSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else
        {
            Debug.LogWarning("Player sprite reference is missing!");
        }
    }

    private void Wait()
    {
        Debug.Log("Player waited for a turn");
    }

    private void Attack()
    {
        Debug.Log($"Attack method called. Current position: {gridPosition}, Facing direction: {facingDirection}");

        if (currentAttackCooldown <= 0)
        {
            Vector2Int attackPosition = GetAttackPosition();

            Debug.Log($"Calculated attack position: {attackPosition}");
            Debug.Log($"Cell type at attack position: {GridManager.Instance.GetCellType(attackPosition)}");

            if (GridManager.Instance.GetCellType(attackPosition) == GridManager.CellType.Enemy)
            {
                EnemyController enemy = FindEnemyAt(attackPosition);
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                    Debug.Log($"Attacked enemy for {attackDamage} damage!");
                    animator.SetTrigger("Attack");
                    currentAttackCooldown = attackCooldown;
                }
                else
                {
                    Debug.Log("Enemy cell detected but no EnemyController found.");
                }
            }
            else
            {
                Debug.Log($"No enemy at attack position. Cell type: {GridManager.Instance.GetCellType(attackPosition)}");
            }
        }
        else
        {
            Debug.Log($"Attack on cooldown. {currentAttackCooldown} turns remaining.");
        }
    }

    private Vector2Int GetAttackPosition()
    {
        Vector2Int frontPosition = gridPosition + facingDirection;
        Debug.Log($"Calculating attack position. Front position: {frontPosition}");

        if (GridManager.Instance.GetCellType(frontPosition) == GridManager.CellType.Enemy)
        {
            Debug.Log($"Enemy found at front position: {frontPosition}");
            return frontPosition;
        }

        if (GridManager.Instance.GetCellType(gridPosition) == GridManager.CellType.Enemy)
        {
            Debug.Log($"Enemy found at current position: {gridPosition}");
            return gridPosition;
        }

        Debug.Log($"No enemy found. Returning front position: {frontPosition}");
        return frontPosition;
    }

    private EnemyController FindEnemyAt(Vector2Int position)
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            if (enemy.GetGridPosition() == position)
            {
                return enemy;
            }
        }
        return null;
    }

    private void SwitchWithEnemy(Vector2Int enemyPosition)
    {
        EnemyController enemy = FindEnemyAt(enemyPosition);
        if (enemy != null)
        {
            Vector2Int playerOldPosition = gridPosition;
            Vector2Int enemyOldFacingDirection = enemy.GetFacingDirection();

            gridPosition = enemyPosition;
            enemy.SetPosition(playerOldPosition);

            GridManager.Instance.SetCellType(playerOldPosition, GridManager.CellType.Enemy);
            GridManager.Instance.SetCellType(enemyPosition, GridManager.CellType.Player);

            StartCoroutine(MoveAnimation(GridManager.Instance.GridToWorldPosition(gridPosition)));

            enemy.UpdatePositionAndDirection(playerOldPosition, enemyOldFacingDirection);

            currentSwitchCooldown = switchCooldown;

            enemy.TakeDamage(attackDamage/2);

            Debug.Log("Player switched positions with enemy");
        }
    }

    private void DecreaseCooldowns()
    {
        if (currentAttackCooldown > 0) currentAttackCooldown--;
        if (currentSwitchCooldown > 0) currentSwitchCooldown--;
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
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Player);
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
        GameManager.Instance.UpdateHealthUI();
        if (currentHealth <= 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            GameManager.Instance.UpdateHealthUI();
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Empty);

        // 禁用玩家控制
        this.enabled = false;

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 显示游戏结束面板
        if (gameOverPanel != null)
        {
            StartCoroutine(ShowGameOverPanelAfterAnimation());
        }
        else
        {
            Debug.LogError("GameOverPanel reference is missing!");
        }
    }

    private IEnumerator ShowGameOverPanelAfterAnimation()
    {
        // 等待死亡动画播放完毕
        if (animator != null)
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        gameOverPanel.Show();
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public int GetAttackCooldown() => currentAttackCooldown;
    public int GetSwitchCooldown() => currentSwitchCooldown;
    public Vector2Int GetGridPosition() => gridPosition;
    public Vector2Int GetFacingDirection() => facingDirection;
}