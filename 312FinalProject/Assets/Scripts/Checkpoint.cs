using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            RaceManager.Instance.CheckpointReached(checkpointIndex);
            Debug.Log($"Current Checkpoint: {checkpointIndex}");
        }
    }
}
