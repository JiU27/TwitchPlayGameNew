using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float turnCountdownDuration = 5f;
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyController enemy;

    private bool isPlayerTurn = true;
    private bool isTurnActive = false;
    private bool isCountdownActive = false;
    private Dictionary<KeyCode, string> keyToAction = new Dictionary<KeyCode, string>
    {
        {KeyCode.A, "MoveLeft"},
        {KeyCode.D, "MoveRight"},
        {KeyCode.W, "Turn"},
        {KeyCode.S, "Wait"},
        {KeyCode.Space, "Attack"}
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
        if (player == null || enemy == null || UIManager.Instance == null)
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
                    UIManager.Instance.UpdateActionCount(action, actionCounts[action]);
                    UIManager.Instance.HighlightButton(action);
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (player != null && enemy != null && UIManager.Instance != null)
        {
            float playerHealth = player.GetHealth();
            float playerMaxHealth = player.GetMaxHealth();
            float enemyHealth = enemy.GetHealth();
            float enemyMaxHealth = enemy.GetMaxHealth();
            UIManager.Instance.UpdateHealthBars(playerHealth, playerMaxHealth, enemyHealth, enemyMaxHealth);

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
        player.PerformAction(selectedAction);

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
        enemy.PerformTurn();

        yield return new WaitForSeconds(1f); // Wait for enemy action
        isTurnActive = false;
        Debug.Log("Enemy turn ended");
    }

    private void ResetActionCounts()
    {
        List<string> keys = new List<string>(actionCounts.Keys);
        foreach (var action in keys)
        {
            actionCounts[action] = 0;
        }
        totalActionCount = 0;
        UIManager.Instance.ResetActionCounts();
    }

    private string GetMostPressedAction()
    {
        string mostPressedAction = "Wait";
        int maxCount = -1;

        foreach (var kvp in actionCounts)
        {
            if (kvp.Value > maxCount)
            {
                mostPressedAction = kvp.Key;
                maxCount = kvp.Value;
            }
        }

        return mostPressedAction;
    }

    public void UpdateHealthUI()
    {
        if (player != null && enemy != null && UIManager.Instance != null)
        {
            float playerHealth = player.GetHealth();
            float playerMaxHealth = player.GetMaxHealth();
            float enemyHealth = enemy.GetHealth();
            float enemyMaxHealth = enemy.GetMaxHealth();
            UIManager.Instance.UpdateHealthBars(playerHealth, playerMaxHealth, enemyHealth, enemyMaxHealth);
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
        if (PMessage.Contains("!a"))
        {
            // Simulate pressing the A key (Move Left)
            PerformChatAction(KeyCode.A);
        }

        if (PMessage.Contains("!aa"))
        {
            // Simulate pressing the A key (Move Left)
            PerformChatAction(KeyCode.A);
        }

        if (PMessage.Contains("!d"))
        {
            // Simulate pressing the D key (Move Right)
            PerformChatAction(KeyCode.D);
        }

        if (PMessage.Contains("!dd"))
        {
            // Simulate pressing the D key (Move Right)
            PerformChatAction(KeyCode.D);
        }

        if (PMessage.Contains("!w"))
        {
            // Simulate pressing the W key (Turn)
            PerformChatAction(KeyCode.W);
        }

        if (PMessage.Contains("!ww"))
        {
            // Simulate pressing the W key (Turn)
            PerformChatAction(KeyCode.W);
        }

        if (PMessage.Contains("!s"))
        {
            // Simulate pressing the S key (Wait)
            PerformChatAction(KeyCode.S);
        }

        if (PMessage.Contains("!ss"))
        {
            // Simulate pressing the S key (Wait)
            PerformChatAction(KeyCode.S);
        }

        if (PMessage.Contains("!j"))
        {
            // Simulate pressing the Space key (Attack)
            PerformChatAction(KeyCode.J);
        }

        if (PMessage.Contains("!jj"))
        {
            // Simulate pressing the Space key (Attack)
            PerformChatAction(KeyCode.J);
        }
    }

    private void PerformChatAction(KeyCode key)
    {
        // This simulates the same logic as if the player pressed the key.
        if (keyToAction.TryGetValue(key, out string action))
        {
            if (actionCounts.ContainsKey(action))
            {
                actionCounts[action]++;
                UIManager.Instance.UpdateActionCount(action, actionCounts[action]);
                UIManager.Instance.HighlightButton(action);

                Debug.Log($"Chat command executed: {action}");
            }
        }
    }
}