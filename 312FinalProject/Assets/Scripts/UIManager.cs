using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuContainer;
    [SerializeField] private GameObject failContainer;
    [SerializeField] private GameObject winContainer;
    [SerializeField] private GameObject timerContainer;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI lastCheckPointTimeText;

    [Header("Win Panel")]
    [SerializeField] private TextMeshProUGUI winTimeText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ShowMainMenu();
    }

    #region Main Menu
    public void OnClickEasy()
    {
        StartGame(1);
    }

    public void OnClickMedium()
    {
        StartGame(2);
    }

    public void OnClickHard()
    {
        StartGame(3);
    }

    public void OnClickQuit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void StartGame(int difficulty)
    {
        Debug.Log("Starting Game with difficulty " + difficulty);
        // Pass difficulty to RaceManager or other scripts
        StartCoroutine(CreateTrack.Instance.BuildTrack(difficulty));

        // Hide all overlays
        mainMenuContainer.SetActive(false);
        failContainer.SetActive(false);
        winContainer.SetActive(false);

        RaceManager.Instance.StartRace();
        timerContainer.SetActive(true);
    }
    #endregion

    #region Fail Panel
    public void ShowFail()
    {
        timerContainer.SetActive(false);
        failContainer.SetActive(true);
    }

    public void OnRetryFromFail()
    {
        failContainer.SetActive(false);
        RaceManager.Instance.ResetRaceAndStart();
        timerContainer.SetActive(true);
    }

    public void OnMainMenuFromFail()
    {
        failContainer.SetActive(false);
        ShowMainMenu();
        CreateTrack.Instance.ClearTrack();
        RaceManager.Instance.ResetRaceStateOnly();
    }
    #endregion

    #region Win Panel
    public void ShowWin(float finalTime)
    {
        timerContainer.SetActive(false);
        winTimeText.text = $"Final Time: {finalTime:0.00}s";
        winContainer.SetActive(true);
    }

    public void OnRetryFromWin()
    {
        winContainer.SetActive(false);
        RaceManager.Instance.ResetRaceAndStart();
        timerContainer.SetActive(true);
    }

    public void OnMainMenuFromWin()
    {
        winContainer.SetActive(false);
        ShowMainMenu();
        CreateTrack.Instance.ClearTrack();
        RaceManager.Instance.ResetRaceStateOnly();
    }
    #endregion
    
    private void ShowMainMenu()
    {
        CreateTrack.Instance.ClearTrack();
        mainMenuContainer.SetActive(true);
        failContainer.SetActive(false);
        winContainer.SetActive(false);
        timerContainer.SetActive(false);
    }
    public void UpdateTimers(float totalTime, float lastCheckpointTime)
    {
        totalTimeText.text = $"Total Time: {FormatTime(totalTime)}";
        lastCheckPointTimeText.text = $"Last Checkpoint Time: {FormatTime(lastCheckpointTime)}";
    }

    private string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        float seconds = (int)(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
