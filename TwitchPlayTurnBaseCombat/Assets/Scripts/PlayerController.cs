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

    private Vector2Int gridPosition;
    private Vector2Int facingDirection;
    private int currentAttackCooldown = 0;
    private int currentSwitchCooldown = 0;
    private int currentHealth;

    private Animator animator;
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
        Vector2Int newPosition = gridPosition + direction;
        if (GridManager.Instance.CanMoveTo(newPosition))
        {
            Vector2Int oldPosition = gridPosition;
            gridPosition = newPosition;
            facingDirection = direction;
            GridManager.Instance.MoveEntity(oldPosition, gridPosition, GridManager.CellType.Player);
            StartCoroutine(MoveAnimation(GridManager.Instance.GridToWorldPosition(gridPosition)));
            //UpdateFacingDirection();
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
    }

    private void Turn()
    {
        facingDirection = -facingDirection; // 反转方向
        UpdateFacingDirection();
        Debug.Log("Player turned around");
    }

    private void Wait()
    {
        // Do nothing, just wait for a turn
        Debug.Log("Player waited for a turn");
    }

    private void Attack()
    {
        if (currentAttackCooldown <= 0)
        {
            Vector2Int attackPosition = gridPosition + facingDirection;
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
            }
            else
            {
                Debug.Log("No enemy in range to attack");
            }
        }
        else
        {
            Debug.Log($"Attack on cooldown. {currentAttackCooldown} turns remaining.");
        }
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

            // 交换位置
            gridPosition = enemyPosition;
            enemy.SetPosition(playerOldPosition);

            // 更新网格
            GridManager.Instance.SetCellType(playerOldPosition, GridManager.CellType.Enemy);
            GridManager.Instance.SetCellType(enemyPosition, GridManager.CellType.Player);

            // 更新玩家位置
            StartCoroutine(MoveAnimation(GridManager.Instance.GridToWorldPosition(gridPosition)));

            // 更新敌人位置和朝向
            enemy.UpdatePositionAndDirection(playerOldPosition, enemyOldFacingDirection);

            // 重置 Switch 冷却时间
            currentSwitchCooldown = switchCooldown;

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
        animator.SetBool("IsMoving", true);
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("IsMoving", false);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = GridManager.Instance.GridToWorldPosition(gridPosition);
        GridManager.Instance.SetCellType(gridPosition, GridManager.CellType.Player);
    }

    private void UpdateFacingDirection()
    {
        // 使用 RotationY 旋转
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
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // TODO: Implement player death logic (e.g., game over screen, restart level)
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public int GetAttackCooldown() => currentAttackCooldown;
    public int GetSwitchCooldown() => currentSwitchCooldown;
    public Vector2Int GetGridPosition() => gridPosition;
    public Vector2Int GetFacingDirection() => facingDirection;
}