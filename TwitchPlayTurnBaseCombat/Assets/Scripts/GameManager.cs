using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float turnCountdownDuration = 5f;
    [SerializeField] private PlayerController player;
    [SerializeField] private List<EnemyController> enemies;
    [SerializeField] private GameWinPanel gameWinPanel;

    private bool isPlayerTurn = true;
    private bool isTurnActive = false;
    private bool isCountdownActive = false;
    private Dictionary<KeyCode, string> keyToAction = new Dictionary<KeyCode, string>
    {
        {KeyCode.A, "A"},
        {KeyCode.D, "D"},
        {KeyCode.W, "W"},
        {KeyCode.S, "S"},
        {KeyCode.J, "J"}
    };
    private Dictionary<string, int> actionCounts;
    private int totalActionCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeActionCounts();
    }

    private void Start()
    {
        if (player == null || enemies.Count == 0 || UIManager.Instance == null)
        {
            Debug.LogError("GameManager: Essential components are missing!");
            return;
        }
        UpdateHealthUI();
        StartCoroutine(GameLoop());
    }

    private void InitializeActionCounts()
    {
        actionCounts = new Dictionary<string, int>();
        foreach (var action in keyToAction.Values)
        {
            actionCounts[action] = 0;
        }
    }

    private void Update()
    {
        if (isCountdownActive)
        {
            CheckPlayerInput();
        }
        UpdateUI();
    }

    private void CheckPlayerInput()
    {
        foreach (var kvp in keyToAction)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                string action = kvp.Value;
                if (actionCounts.ContainsKey(action))
                {
                    actionCounts[action]++;
                    totalActionCount++;
                    UIManager.Instance.UpdateActionCount(action, actionCounts[action], totalActionCount);
                    UIManager.Instance.HighlightButton(action);
                    Debug.Log($"Action {action} pressed. Count: {actionCounts[action]}. Total actions: {totalActionCount}");
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (player != null && enemies.Count > 0 && UIManager.Instance != null)
        {
            float playerHealth = player.GetHealth();
            float playerMaxHealth = player.GetMaxHealth();
            UIManager.Instance.UpdatePlayerHealthBar(playerHealth, playerMaxHealth);
            UIManager.Instance.UpdateEnemyHealthBars(enemies);

            int attackCooldown = player.GetAttackCooldown();
            int switchCooldown = player.GetSwitchCooldown();
            UIManager.Instance.UpdateCooldowns(attackCooldown, switchCooldown);
        }
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            yield return StartCoroutine(PlayerTurnCountdown());
            yield return StartCoroutine(PlayerTurn());
            yield return StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator PlayerTurnCountdown()
    {
        Debug.Log("Starting player turn countdown");
        ResetActionCounts();
        UIManager.Instance.ShowActionPanel(true);
        isCountdownActive = true;
        float countdownTime = turnCountdownDuration;

        while (countdownTime > 0)
        {
            UIManager.Instance.UpdateTurnCountdown(countdownTime);
            yield return new WaitForSeconds(0.1f);
            countdownTime -= 0.1f;
        }

        isCountdownActive = false;
        UIManager.Instance.SetTurnCountdownText("Player Turn!");
        UIManager.Instance.ShowActionPanel(false);
        Debug.Log("Player turn countdown ended");
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("Starting player turn");
        isTurnActive = true;
        string selectedAction = GetMostPressedAction();
        Debug.Log($"Selected action: {selectedAction}");

        UIManager.Instance.ShowActionResult(selectedAction);
        yield return new WaitForSeconds(UIManager.Instance.GetActionResultDisplayDuration());

        string playerAction = ConvertActionToPlayerAction(selectedAction);
        player.PerformAction(playerAction);

        yield return new WaitForSeconds(1f); // Wait for action animation
        isTurnActive = false;
        Debug.Log("Player turn ended");
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Starting enemy turn");
        UIManager.Instance.SetTurnCountdownText("Enemy Turn!");
        yield return new WaitForSeconds(0.5f);

        isTurnActive = true;
        foreach (var enemy in enemies.ToList()) // Use ToList to allow for enemy removal during iteration
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.PerformTurn();
                yield return new WaitForSeconds(1f); // Wait between enemy actions
            }
        }

        isTurnActive = false;
        Debug.Log("Enemy turn ended");
    }

    private void ResetActionCounts()
    {
        foreach (var action in actionCounts.Keys.ToList())
        {
            actionCounts[action] = 0;
        }
        totalActionCount = 0;
        UIManager.Instance.ResetActionCounts();
    }

    private string GetMostPressedAction()
    {
        return actionCounts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private string ConvertActionToPlayerAction(string action)
    {
        switch (action)
        {
            case "A": return "MoveLeft";
            case "D": return "MoveRight";
            case "W": return "Turn";
            case "S": return "Wait";
            case "J": return "Attack";
            default: return "Wait";
        }
    }

    public void UpdateHealthUI()
    {
        if (player != null && enemies.Count > 0 && UIManager.Instance != null)
        {
            float playerHealth = player.GetHealth();
            float playerMaxHealth = player.GetMaxHealth();
            UIManager.Instance.UpdatePlayerHealthBar(playerHealth, playerMaxHealth);
            UIManager.Instance.UpdateEnemyHealthBars(enemies);
        }
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
    }

    public bool IsTurnActive()
    {
        return isTurnActive;
    }

    public void OnChatMessage(string PCharacter, string PMessage)
    {
        if (isCountdownActive)
        {
            if (PMessage.Contains("!a"))
            {
                PerformChatAction(KeyCode.A);
            }
            if (PMessage.Contains("!aa"))
            {
                PerformChatAction(KeyCode.A);
            }
            if (PMessage.Contains("!d"))
            {
                PerformChatAction(KeyCode.D);
            }
            if (PMessage.Contains("!dd"))
            {
                PerformChatAction(KeyCode.D);
            }
            if (PMessage.Contains("!w"))
            {
                PerformChatAction(KeyCode.W);
            }
            if (PMessage.Contains("!ww"))
            {
                PerformChatAction(KeyCode.W);
            }
            if (PMessage.Contains("!s"))
            {
                PerformChatAction(KeyCode.S);
            }
            if (PMessage.Contains("!ss"))
            {
                PerformChatAction(KeyCode.S);
            }
            if (PMessage.Contains("!j"))
            {
                PerformChatAction(KeyCode.J);
            }
            if (PMessage.Contains("!jj"))
            {
                PerformChatAction(KeyCode.J);
            }
        }
    }

    private void PerformChatAction(KeyCode key)
    {
        if (keyToAction.TryGetValue(key, out string action))
        {
            if (actionCounts.ContainsKey(action))
            {
                actionCounts[action]++;
                totalActionCount++;
                UIManager.Instance.UpdateActionCount(action, actionCounts[action], totalActionCount);
                UIManager.Instance.HighlightButton(action);
                Debug.Log($"Chat command executed: {action}. Count: {actionCounts[action]}. Total actions: {totalActionCount}");
            }
        }
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        enemies.Remove(enemy);
        UIManager.Instance.UpdateEnemyHealthBars(enemies);
        if (enemies.Count == 0)
        {
            TriggerVictory();
        }
    }

    private void TriggerVictory()
    {
        Debug.Log("Victory! All enemies defeated.");
        StartCoroutine(ShowVictoryPanelWithDelay(1.5f));
    }

    private IEnumerator ShowVictoryPanelWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameWinPanel.Show();
    }

    public void HandleEnemyConflict(EnemyController attacker, EnemyController victim, int damage)
    {
        Debug.Log($"Enemy conflict: {attacker.name} attacked {victim.name} for {damage} damage.");
        // 这里可以添加额外的逻辑，比如更新UI、触发特殊事件等
    }
}