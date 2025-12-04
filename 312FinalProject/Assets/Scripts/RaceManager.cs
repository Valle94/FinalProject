using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [SerializeField] private int lastCheckpointIndex = -1;
    [SerializeField] private TextMeshProUGUI lastCheckPointTimeText;
    [SerializeField] private TextMeshProUGUI totalTimeText;

    private float lastCheckpointTime;
    private float totalTime;
    
    [HideInInspector] public bool raceStarted = false;
    private bool raceFinished = false;
    public List<Checkpoint> checkpoints;

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

    void Update()
    {
        if (raceStarted)
        {
            UpdateTime();
        }

        UpdateUI();
    }

    public void CheckpointReached(int checkpointIndex)
    {
        if ((!raceStarted && checkpointIndex != 0) || raceFinished) return;

        if (checkpointIndex == lastCheckpointIndex + 1)
        {
            UpdateCheckpoint(checkpointIndex);
            if (checkpointIndex % 10 == 0 && checkpointIndex > 1)
            {
                lastCheckpointTime = totalTime;
                lastCheckPointTimeText.text = $"Last Checkpoint Time: {FormatTime(totalTime)}";
            }
        }
    }

    private void UpdateCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex == 0)
        {
            StartRace();
        }
        else if (checkpointIndex == checkpoints.Count -1)
        {
            EndRace();
        }

        lastCheckpointIndex = checkpointIndex;
    }

    public void StartRace()
    {
        raceStarted = true;
        raceFinished = false;
    }

    private void EndRace()
    {
        raceStarted = false;
        raceFinished = true;
    }

    private void UpdateTime()
    {
        totalTime += Time.deltaTime;
    }

    private void UpdateUI()
    {
        totalTimeText.text = $"Total Time: {FormatTime(totalTime)}";
    }
    
    private string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        float seconds = time % 60;
        return string.Format($"{minutes:00}:{seconds:00}");
    }
}
