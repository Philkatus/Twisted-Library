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

    public bool volcanoLandmark;

    bool isSynced;


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
            if(volcanoLandmark) //volcanolandmark
            {
                RotateNormal();
            }
            else //windlandmark
            {
                if(isSynced)
                {
                    Debug.Log("not synced");
                    RotateNormal();
                }
                else //isSyncing
                {
                    if (objectsToSyncWith.Count != 0)
                    {
                        GameObject gameObjectToSyncWith = null;
                        if (objectsToSyncWith[0].GetComponent<RotateStuffEaselyDude>().isPlaying)
                        {
                            gameObjectToSyncWith = objectsToSyncWith[0];
                        }
                        else if (objectsToSyncWith[1].GetComponent<RotateStuffEaselyDude>().isPlaying)
                        {
                            gameObjectToSyncWith = objectsToSyncWith[1];
                        }
                        else
                        {
                            //is the first one, needs no syncing
                            isSynced = true;
                        }

                        if(gameObjectToSyncWith != null)
                        {
                            if (this.transform.localRotation.z <= gameObjectToSyncWith.transform.localRotation.z + 0.5f &&
                            this.transform.localRotation.z >= gameObjectToSyncWith.transform.localRotation.z - 0.5f)
                            {
                                isSynced = true;
                                Debug.Log("Synced");
                                Debug.Log("this.transform.rotation.z " + this.transform.localRotation.z + " &  gameObjectToSyncWith.transform.rotation.z " + gameObjectToSyncWith.transform.localRotation.z);
                            }
                        }
                        
                    }

                    RotateSlower();
                }  
            }
        }
    }

    public void RotateNormal()
    {
        if (rotateThisTransform)
        {
            this.transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0), Space.World);
        }
        else
        {
            transformToRotate.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0), Space.World);
        }
    }

    public void RotateSlower()
    {
        if (rotateThisTransform)
        {
            this.transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime / 2, 0), Space.World);
        }
        else
        {
            transformToRotate.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime / 2, 0), Space.World);
        }
    }

    
}
