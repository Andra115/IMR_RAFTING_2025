using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelHud;
    public GameObject panelPause;
    public TMP_Text timerText;
    public TMP_Text scoreText; // Add this

    float time;
    bool running;
    bool paused;
    float savedTimeScale;
    int score = 0; // Add this

    void Start()
    {
        paused = false;
        savedTimeScale = 1f;
        time = 0f;
        running = false;
        score = 0; // Add this

        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelHud != null) panelHud.SetActive(false);
        if (panelPause != null) panelPause.SetActive(false);

        Time.timeScale = 1f;

        if (timerText != null)
            timerText.text = "Time: 00:00.00";

        if (scoreText != null) // Add this
            scoreText.text = "Score: 0";
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

    // Add this method
    public void AddScore(int points)
    {
        score += points;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void StartGame()
    {
        paused = false;
        Time.timeScale = 1f;

        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelHud != null) panelHud.SetActive(true);
        if (panelPause != null) panelPause.SetActive(false);

        time = 0f;
        score = 0; // Add this
        running = true;

        if (timerText != null)
            timerText.text = "Time: 00:00.00";

        if (scoreText != null) // Add this
            scoreText.text = "Score: 0";
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