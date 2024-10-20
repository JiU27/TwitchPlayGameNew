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
    [SerializeField] private Transform spriteTransform;

    public GameObject particleEffectPrefab;
    public Transform spawnPosition;

    private Vector2Int gridPosition;
    private Vector2Int facingDirection;
    private int currentAttackCooldown = 0;
    private int currentSwitchCooldown = 0;
    private int currentHealth;

    private SpriteRenderer spriteRenderer;

    public AudioSource player_hurt;

    [SerializeField] private float switchAnimationDuration = 0.5f;

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
        UpdateFacingDirection();
        Debug.Log("Player turned around");
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
                    animator.SetTrigger("Attack");
                    enemy.TakeDamage(attackDamage);
                    Debug.Log($"Attacked enemy for {attackDamage} damage!");
                    currentAttackCooldown = attackCooldown;
                }
                else
                {
                    Debug.Log("Enemy cell detected but no EnemyController found.");
                }
            }
            else
            {
                animator.SetTrigger("Attack");
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

            // 开始交换位置动画
            StartCoroutine(SwitchPositionAnimation(enemy, playerOldPosition, enemyPosition));

            currentSwitchCooldown = switchCooldown;

            Debug.Log("Player switched positions with enemy");
        }
    }

    private IEnumerator SwitchPositionAnimation(EnemyController enemy, Vector2Int playerOldPosition, Vector2Int enemyOldPosition)
    {
        if (animator != null)
        {
            animator.SetTrigger("Switch");
        }

        Vector3 midPoint = (GridManager.Instance.GridToWorldPosition(playerOldPosition) +
                            GridManager.Instance.GridToWorldPosition(enemyOldPosition)) / 2;

        yield return StartCoroutine(MoveToPosition(midPoint));

        gridPosition = enemyOldPosition;
        enemy.SetPosition(playerOldPosition);

        GridManager.Instance.SetCellType(playerOldPosition, GridManager.CellType.Enemy);
        GridManager.Instance.SetCellType(enemyOldPosition, GridManager.CellType.Player);

        yield return StartCoroutine(MoveToPosition(GridManager.Instance.GridToWorldPosition(gridPosition)));

        enemy.UpdatePositionAndDirection(playerOldPosition, enemy.GetFacingDirection());

        //enemy.TakeDamage(attackDamage / 2);

        UpdatePosition();
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
        if (spriteTransform != null)
        {
            if (facingDirection.x < 0)
            {
                spriteTransform.localScale = new Vector3(-3.7f, 3.7f, 3.7f);
            }
            else if (facingDirection.x > 0)
            {
                spriteTransform.localScale = new Vector3(3.7f, 3.7f, 3.7f);
            }
        }
        else
        {
            Debug.LogWarning("Sprite transform reference is missing!");
        }
    }

    public void TakeDamage(int damage)
    {
        player_hurt.Play();
        GameObject particleInstance = Instantiate(particleEffectPrefab, spawnPosition.position, spawnPosition.rotation);

        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
        Destroy(particleInstance, particleSystem.main.duration);

        currentHealth = Mathf.Max(0, currentHealth - damage);
        GameManager.Instance.UpdateHealthUI();

        EffectsManager.Instance.PlayPlayerDamageEffects(transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 在PlayerController类中添加此方法
    public void DealDamage(EnemyController enemy, int damage)
    {
        bool isFacingRight = facingDirection.x > 0;
        enemy.TakeDamage(damage, isFacingRight);
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