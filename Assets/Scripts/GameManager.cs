using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;

    void Update()
    {
        // ��Ǩ�ͺ��á����� ESCs
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false); // �Դ˹�� Setting ���¶���Դ��ҧ���
        Time.timeScale = 1f; // �����������Թ����
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // ��ش�������������
        isPaused = true;
    }

    // �ѧ��ѹ����Ѻ���� Setting
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    // �ѧ��ѹ����Ѻ���� Back �˹�� Setting
    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}
