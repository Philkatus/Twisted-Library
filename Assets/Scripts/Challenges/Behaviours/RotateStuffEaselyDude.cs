using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateStuffEaselyDude : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    public bool isPlaying;
    [SerializeField] bool rotateThisTransform;
    [SerializeField] Transform transformToRotate;

    [SerializeField] List<GameObject> objectsToSyncWith = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlaying)
        {
            if(rotateThisTransform)
            {
                this.transform.Rotate(new Vector3(0, rotationSpeed, 0), Space.World);
            }
            else
            {
                transformToRotate.Rotate(new Vector3(0, rotationSpeed, 0), Space.World);
            }

            if(objectsToSyncWith.Count != 0)
            {

            }
        }
    }
}
