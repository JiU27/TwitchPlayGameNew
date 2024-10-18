using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private TextMeshProUGUI attackCooldownText;
    [SerializeField] private TextMeshProUGUI switchCooldownText;
    [SerializeField] private TextMeshProUGUI turnCountdownText;
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private Button[] actionButtons;
    [SerializeField] private Slider[] actionCountSliders;
    [SerializeField] private ActionResultPanel actionResultPanel;

    private Dictionary<string, Slider> actionSliderMap = new Dictionary<string, Slider>();
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
        InitializeActionSliderMap();
    }

    private void InitializeActionSliderMap()
    {
        string[] actionKeys = { "A", "D", "W", "S", "J" };
        for (int i = 0; i < actionCountSliders.Length; i++)
        {
            actionSliderMap[actionKeys[i]] = actionCountSliders[i];
        }
    }

    public void UpdatePlayerHealthBar(float playerHealth, float playerMaxHealth)
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.value = playerHealth / playerMaxHealth;
        }
    }

    public void UpdateEnemyHealthBars(List<EnemyController> enemies)
    {
        foreach (var enemy in enemies)
        {
            UpdateEnemyHealthBar(enemy);
        }
    }

    private void UpdateEnemyHealthBar(EnemyController enemy)
    {
        Slider healthBar = enemy.GetComponentInChildren<Slider>();
        if (healthBar != null)
        {
            healthBar.value = enemy.GetHealth() / enemy.GetMaxHealth();
        }
    }

    public void UpdateCooldowns(int attackCooldown, int switchCooldown)
    {
        attackCooldownText.text = $"Attack Cooldown: {attackCooldown}";
        switchCooldownText.text = $"Switch Cooldown: {switchCooldown}";
    }

    public void UpdateTurnCountdown(float countdownTime)
    {
        turnCountdownText.text = $"Time: {countdownTime:F1}";
    }

    public void SetTurnCountdownText(string text)
    {
        turnCountdownText.text = text;
    }

    public void ShowActionPanel(bool show)
    {
        actionPanel.SetActive(show);
        if (!show)
        {
            ResetActionCounts();
        }
    }

    public void UpdateActionCount(string action, int count, int totalCount)
    {
        UpdateSliderMaxValues(totalCount);

        if (actionSliderMap.TryGetValue(action, out Slider slider))
        {
            slider.value = count;
        }

        Debug.Log($"Action {action} pressed. Count: {count}. Total actions: {totalCount}");
    }

    private void UpdateSliderMaxValues(int totalCount)
    {
        int maxValue = Mathf.Max(1, totalCount);
        foreach (var slider in actionCountSliders)
        {
            slider.maxValue = maxValue;
        }
    }

    public void ResetActionCounts()
    {
        foreach (var slider in actionCountSliders)
        {
            slider.value = 0;
            slider.maxValue = 1;
        }
        totalActionCount = 0;
    }

    public void HighlightButton(string action)
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i].name.Replace("Button", "") == action)
            {
                StartCoroutine(FlashButton(actionButtons[i]));
                break;
            }
        }
    }

    private IEnumerator FlashButton(Button button)
    {
        Color originalColor = button.image.color;
        button.image.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        button.image.color = originalColor;
    }

    public void ShowActionResult(string action)
    {
        if (actionResultPanel != null)
        {
            actionResultPanel.ShowResult(action);
        }
        else
        {
            Debug.LogError("ActionResultPanel reference is missing in UIManager.");
        }
    }

    public float GetActionResultDisplayDuration()
    {
        return actionResultPanel.GetDisplayDuration();
    }
}