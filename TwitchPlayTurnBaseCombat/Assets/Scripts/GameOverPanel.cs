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
        // �ȴ�ָ��ʱ�䣬��������������
        yield return new WaitForSecondsRealtime(delayBeforeText);

        // ��ͣ��Ϸ
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
        // �ָ�����ʱ������
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToReload);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // ȷ������屻����ʱ�ָ�ʱ������
        Time.timeScale = 1f;
    }
}