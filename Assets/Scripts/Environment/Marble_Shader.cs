using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marble_Shader : MonoBehaviour
{
    [SerializeField] Transform murmel;
    [SerializeField] Transform trackParent;
    Transform[] tracks;
    Dictionary<Transform, Material> assigendMaterials = new Dictionary<Transform, Material>();
    Dictionary<Transform, float> objectLengths = new Dictionary<Transform, float>();
    Dictionary<Transform, Vector3> trackStart = new Dictionary<Transform, Vector3>();
    Material currentMat;
    int currentTrackIndex = 0;

    private void Start()
    {

        List<Transform> trackList = new List<Transform>();
        tracks = trackParent.GetComponentsInChildren<Transform>();
        trackList.AddRange(tracks);
        trackList.RemoveAt(0);
        tracks = trackList.ToArray();
        foreach (Transform t in tracks)
        {
            float length = t.localScale.y * 2;
            currentMat = new Material(t.GetComponent<MeshRenderer>().material);
            t.GetComponent<MeshRenderer>().material = currentMat;
            assigendMaterials.Add(t, currentMat);
            objectLengths.Add(t, length);
            //currentMat.SetFloat("Float_Length", length / 8);
            trackStart.Add(t, t.position - t.up.normalized * length / 2);
        }

    }

    private void Update()
    {
        float distance = 0;
        float localDistance = 0;
        Transform track = tracks[currentTrackIndex];

        distance = Vector3.Distance(murmel.transform.position, trackStart[track]);
        localDistance = ExtensionMethods.Remap(distance, 0, objectLengths[track], 0, 1);

        if (distance >= objectLengths[track])
        {
            localDistance = 1;
            if (currentTrackIndex < tracks.Length - 2)
                currentTrackIndex++;
            else
                ResetRail();
        }

        assigendMaterials[track].SetFloat("Float_BallDistance", localDistance);
    }
    private void ResetRail()
    {
    }
}
