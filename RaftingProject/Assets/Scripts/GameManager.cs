using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelHud;
    public GameObject panelPause;
    public TMP_Text timerText;

    float time;
    bool running;
    bool paused;
    float savedTimeScale;

    void Start()
    {
        paused = false;
        savedTimeScale = 1f;
        time = 0f;
        running = false;

        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelHud != null) panelHud.SetActive(false);
        if (panelPause != null) panelPause.SetActive(false);

        Time.timeScale = 1f;

        if (timerText != null)
            timerText.text = "Time: 00:00.00";
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            FinishGame();
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (!running || paused) return;

        time += Time.unscaledDeltaTime;

        int min = Mathf.FloorToInt(time / 60f);
        float sec = time % 60f;

        if (timerText != null)
            timerText.text = "Time: " + min.ToString("00") + ":" + sec.ToString("00.00");
    }

    public void StartGame()
    {
        paused = false;
        Time.timeScale = 1f;

        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelHud != null) panelHud.SetActive(true);
        if (panelPause != null) panelPause.SetActive(false);

        time = 0f;
        running = true;

        if (timerText != null)
            timerText.text = "Time: 00:00.00";
    }

    public void FinishGame()
    {
        running = false;
        paused = false;
        Time.timeScale = 1f;

        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelHud != null) panelHud.SetActive(true);
        if (panelPause != null) panelPause.SetActive(false);
    }

    void TogglePause()
    {
        if (!running) return;

        paused = !paused;

        if (paused)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (panelPause != null)
                panelPause.SetActive(true);

            if (timerText != null)
            {
                int min = Mathf.FloorToInt(time / 60f);
                float sec = time % 60f;
                timerText.text = "PAUSED - " + min.ToString("00") + ":" + sec.ToString("00.00");
            }
        }
        else
        {
            Time.timeScale = savedTimeScale;

            if (panelPause != null)
                panelPause.SetActive(false);

            if (timerText != null)
            {
                int min = Mathf.FloorToInt(time / 60f);
                float sec = time % 60f;
                timerText.text = "Time: " + min.ToString("00") + ":" + sec.ToString("00.00");
            }
        }
    }
}
