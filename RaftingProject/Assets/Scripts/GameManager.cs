using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelHud;
    public TMP_Text timerText;

    float time;
    bool running;

    void Start()
    {
        panelMenu.SetActive(true);
        panelHud.SetActive(false);
        time = 0f;
        running = false;

        timerText.text = "Time: 00:00.00";
    }

    void Update()
    {
        if (!running) return;

        time += Time.deltaTime;

        int min = Mathf.FloorToInt(time / 60f);
        float sec = time % 60f;

        timerText.text = "Time: " + min.ToString("00") + ":" + sec.ToString("00.00");
    }

    public void StartGame()
    {
        panelMenu.SetActive(false);
        panelHud.SetActive(true);
        time = 0f;
        running = true;
    }
}
