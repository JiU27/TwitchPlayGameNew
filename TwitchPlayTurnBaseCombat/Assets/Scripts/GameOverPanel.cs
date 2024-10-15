using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private float delayBeforeText = 2f;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private string sceneToReload;

    private bool canRestart = false;

    private void OnEnable()
    {
        gameOverText.gameObject.SetActive(false);
        StartCoroutine(ShowTextAfterDelay());
    }

    private IEnumerator ShowTextAfterDelay()
    {
        // 等待指定时间，让死亡动画播放
        yield return new WaitForSecondsRealtime(delayBeforeText);

        // 暂停游戏
        Time.timeScale = 0f;

        gameOverText.gameObject.SetActive(true);
        canRestart = true;
    }

    private void Update()
    {
        if (canRestart && Input.GetKeyDown(KeyCode.J))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        // 恢复正常时间流逝
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToReload);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // 确保在面板被禁用时恢复时间流逝
        Time.timeScale = 1f;
    }
}