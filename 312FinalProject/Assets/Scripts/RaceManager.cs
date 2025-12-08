using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [SerializeField] private int lastCheckpointIndex = -1;
    [SerializeField] private GameObject player;

    private float lastCheckpointTime;
    private float totalTime;
    private float timeSinceGrounded = 0f;
    private float timeSinceLastCheckpoint = 0f;

    [HideInInspector] public bool raceStarted = false;
    private bool raceFinished = false;
    private bool failTriggered = false;

    private Movement playerMovement;
    public List<Checkpoint> checkpoints;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerMovement = player.GetComponent<Movement>();
    }

    void Update()
    {
        if (raceStarted)
        {
            UpdateTime();
            UIManager.Instance.UpdateTimers(totalTime, lastCheckpointTime);
        }
    }

    #region Checkpoints
    public void CheckpointReached(int checkpointIndex)
    {
        if ((!raceStarted && checkpointIndex != 0) || raceFinished) return;

        // Always apply checkpoint logic
        UpdateCheckpoint(checkpointIndex);

        if (checkpointIndex % 10 == 0 && checkpointIndex > 1)
        {
            lastCheckpointTime = totalTime;  // still store it
            UIManager.Instance.UpdateTimers(totalTime, lastCheckpointTime); // notify UI
        }
    }

    private void UpdateCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex == checkpoints.Count - 1)
        {
            EndRace();
            UIManager.Instance.ShowWin(totalTime);
            return;
        }

        if (checkpointIndex == 0 && !raceStarted)
        {
            StartRace(); // Only auto-start for first checkpoint if race not started
        }

        timeSinceLastCheckpoint = 0;
        lastCheckpointIndex = checkpointIndex;
    }
    #endregion

    #region Race Control
    public void StartRace()
    {
        raceStarted = true;
        raceFinished = false;
        failTriggered = false;
        playerMovement.StartPlayer();
    }

    private void EndRace()
    {
        raceStarted = false;
        raceFinished = true;
    }

    public void ResetRaceStateOnly()
    {
        raceStarted = false;
        raceFinished = false;
        failTriggered = false;

        totalTime = 0f;
        timeSinceGrounded = 0f;
        timeSinceLastCheckpoint = 0f;
        lastCheckpointTime = 0f;

        lastCheckpointIndex = -1;

        playerMovement.ResetPlayer();
        playerMovement.StopPlayer();
    }

    public void ResetRaceAndStart()
    {
        ResetRaceStateOnly();
        StartRace();
    }
    #endregion

    #region Timing
    private void UpdateTime()
    {
        totalTime += Time.deltaTime;

        if (!playerMovement.isGrounded)
            timeSinceGrounded += Time.deltaTime;
        else
            timeSinceGrounded = 0f;

        timeSinceLastCheckpoint += Time.deltaTime;

        // Fail condition
        if (!failTriggered && (timeSinceGrounded > 5f || timeSinceLastCheckpoint > 10f))
        {
            failTriggered = true;
            raceStarted = false;           // pause race
            playerMovement.StopPlayer();
            UIManager.Instance.ShowFail(); // show fail overlay
        }
    }
    #endregion
}