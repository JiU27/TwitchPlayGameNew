using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ActionResultPanel : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField]
    private List<string> actionTexts = new List<string>
    {
        "A key pressed: {0}",
        "D key pressed: {0}",
        "W key pressed: {0}",
        "S key pressed: {0}",
        "J key pressed: {0}"
    };

    private void Awake()
    {
        // 确保这个脚本所在的游戏对象始终是激活的
        gameObject.SetActive(true);
        // 初始时隐藏结果面板
        HidePanel();
    }

    public void ShowResult(string action)
    {
        // 使用非协程方法来显示结果
        ShowPanel();
        UpdateResultText(action);
        StartCoroutine(HidePanelAfterDelay());
    }

    private void UpdateResultText(string action)
    {
        int index = "ADWSJ".IndexOf(action);
        if (index >= 0 && index < actionTexts.Count)
        {
            resultText.text = string.Format(actionTexts[index], action);
        }
        else
        {
            resultText.text = $"Action performed: {action}";
        }
    }

    private void ShowPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Result panel is not assigned in ActionResultPanel script.");
        }
    }

    private void HidePanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private IEnumerator HidePanelAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        HidePanel();
    }

    public float GetDisplayDuration()
    {
        return displayDuration;
    }
}