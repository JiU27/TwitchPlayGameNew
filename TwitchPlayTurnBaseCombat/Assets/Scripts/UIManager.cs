using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private Slider enemyHealthBar;
    [SerializeField] private TextMeshProUGUI attackCooldownText;
    [SerializeField] private TextMeshProUGUI switchCooldownText;
    [SerializeField] private TextMeshProUGUI turnCountdownText;
    [SerializeField] private GameObject actionPanel;
    [SerializeField] private Button[] actionButtons;
    [SerializeField] private TextMeshProUGUI[] actionCountTexts;

    private Dictionary<string, TextMeshProUGUI> actionTextMap = new Dictionary<string, TextMeshProUGUI>();

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

        InitializeActionTextMap();
    }

    private void InitializeActionTextMap()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            string actionName = actionButtons[i].name.Replace("Button", "");
            actionTextMap[actionName] = actionCountTexts[i];
        }
    }

    public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.value = playerHealth / playerMaxHealth;
        }
        if (enemyHealthBar != null)
        {
            enemyHealthBar.value = enemyHealth / enemyMaxHealth;
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
    }

    public void UpdateActionCount(string action, int count)
    {
        if (actionTextMap.TryGetValue(action, out TextMeshProUGUI text))
        {
            text.text = count.ToString();
        }
        UpdateHighestCountColor();
    }

    private void UpdateHighestCountColor()
    {
        int maxCount = actionCountTexts.Max(text => int.Parse(text.text));
        foreach (var text in actionCountTexts)
        {
            text.color = int.Parse(text.text) == maxCount ? Color.red : Color.white;
        }
    }

    public void ResetActionCounts()
    {
        foreach (var text in actionCountTexts)
        {
            text.text = "0";
            text.color = Color.black;
        }
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
}