using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateTrack : MonoBehaviour
{
    public static CreateTrack Instance;

    [Range(10, 300)] public int trackLength = 10;
    [SerializeField] GameObject startPiece;
    [SerializeField] GameObject endPiece;
    [SerializeField] GameObject[] middlePieces;
    [SerializeField] GameObject player;

    List<GameObject> CreatedPieces = new List<GameObject>();
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    private void Awake() 
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

    void Start()
    {
        // StartCoroutine(BuildTrack());
        // Instantiate(player, transform.position, Quaternion.identity);
    }

    public IEnumerator BuildTrack(int diffulty)
    {
        trackLength = 10 * diffulty;
        SpawnStart();
        yield return null;

        while (CreatedPieces.Count < trackLength)
        {
            SpawnPiece();
            yield return null;
        }
        SpawnEnd();
    }

    public void ClearTrack()
    {
        foreach (GameObject piece in CreatedPieces)
            Destroy(piece);

        CreatedPieces.Clear();
        occupiedCells.Clear();

        // Also wipe checkpoints from RaceManager
        RaceManager.Instance.checkpoints.Clear();
    }

    private void SpawnStart()
    {
        GameObject startLine = Instantiate(startPiece, transform.position, Quaternion.identity);
        CreatedPieces.Add(startLine);
        var tp = startLine.GetComponent<TrackPiece>();
        occupiedCells.Add(QuantizePosition(tp.frontTransform.position));
    }

    private void SpawnEnd()
    {
        GameObject lastPiece = CreatedPieces.Last();
        TrackPiece lastTrack = lastPiece.GetComponent<TrackPiece>();

        Transform connectPoint = lastTrack.frontTransform;

        // Instantiate new piece in a temporary pose
        GameObject newPiece = Instantiate(endPiece, Vector3.zero, Quaternion.identity);
        TrackPiece newTrack = newPiece.GetComponent<TrackPiece>();

        // 1. Align rotation
        newPiece.transform.rotation =
            connectPoint.rotation * Quaternion.Inverse(newTrack.backTransform.localRotation);

        // 2. Align position
        Vector3 offset = newTrack.backTransform.position - newPiece.transform.position;
        newPiece.transform.position = connectPoint.position - offset;
 
        // 3. FOOTPRINT OVERLAP CHECK
        if (IsOverlapping(newTrack))
        {
            Destroy(newPiece);
            Destroy(CreatedPieces.Last());
            CreatedPieces.Remove(CreatedPieces.Last());
            foreach (GameObject gameoobject in CreatedPieces)
            {
                Debug.Log(gameObject.ToString());
            }
            RaceManager.Instance.checkpoints.RemoveAt(RaceManager.Instance.checkpoints.Count -1);
            Debug.Log("InvalidPiece");
            return; // INVALID — OVERLAP DETECTED
        }

        // 4. Accept piece
        CreatedPieces.Add(newPiece);

        newPiece.GetComponent<TrackPiece>().Checkpoint.checkpointIndex = CreatedPieces.IndexOf(lastPiece);
        RaceManager.Instance.checkpoints.Add(newPiece.GetComponent<TrackPiece>().Checkpoint);
    }

    public void OnInteract()
    {
        SpawnPiece();
    }

    private void SpawnPiece()
    {
        int randInt = Random.Range(0, middlePieces.Length);

        GameObject lastPiece = CreatedPieces.Last();
        TrackPiece lastTrack = lastPiece.GetComponent<TrackPiece>();

        Transform connectPoint = lastTrack.frontTransform;

        // Instantiate new piece in a temporary pose
        GameObject newPiece = Instantiate(middlePieces[randInt], Vector3.zero, Quaternion.identity);
        TrackPiece newTrack = newPiece.GetComponent<TrackPiece>();

        // 1. Align rotation
        newPiece.transform.rotation =
            connectPoint.rotation * Quaternion.Inverse(newTrack.backTransform.localRotation);

        // 2. Align position
        Vector3 offset = newTrack.backTransform.position - newPiece.transform.position;
        newPiece.transform.position = connectPoint.position - offset;
 
        // 3. FOOTPRINT OVERLAP CHECK
        if (IsOverlapping(newTrack))
        {
            Destroy(newPiece);
            if (CreatedPieces.Count > 1)
            {
                Destroy(CreatedPieces.Last());
                CreatedPieces.Remove(CreatedPieces.Last());
                RaceManager.Instance.checkpoints.RemoveAt(RaceManager.Instance.checkpoints.Count -1);
            }
            Debug.Log("InvalidPiece");
            return; // INVALID — OVERLAP DETECTED
        }

        // 4. Accept piece
        CreatedPieces.Add(newPiece);
        newPiece.GetComponent<TrackPiece>().Checkpoint.checkpointIndex = CreatedPieces.IndexOf(lastPiece);
        RaceManager.Instance.checkpoints.Add(newPiece.GetComponent<TrackPiece>().Checkpoint);
    }

    private bool IsOverlapping(TrackPiece newTrack)
    {
        BoxCollider box = newTrack.pieceArea;

        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 halfSize = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
        Quaternion rotation = box.transform.rotation;

        Collider[] hits = Physics.OverlapBox(center, halfSize, rotation);

        foreach (var h in hits)
        {
            // Ignore itself
            if (h == box) continue;

            // Ignore the previous piece (close alignment causes false positives)
            if (h.transform.root == CreatedPieces.Last().transform) continue;

            // Ignore anything not tagged as TrackPiece
            if (!h.transform.root.CompareTag("TrackPiece")) continue;

            Debug.Log("OVERLAP WITH " + h.name);
            return true;
        }

        return false;
    }

    public void OnCrouch()
    {
        if (CreatedPieces.Count > 1)
        {
            DestroyPiece();
        }
    }

    private void DestroyPiece()
    {
        Destroy(CreatedPieces.Last());
        CreatedPieces.RemoveAt(CreatedPieces.Count - 1);
    }

    private Vector3Int QuantizePosition(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y),
            Mathf.RoundToInt(pos.z)
        );
    }
}