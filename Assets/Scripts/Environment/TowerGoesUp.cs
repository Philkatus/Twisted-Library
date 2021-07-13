using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGoesUp : MonoBehaviour
{

    [SerializeField] Vector3 endPosition;
    [SerializeField] float travelTime;
    float time;
    Vector3 startPos, midwayPos;

    bool sendTowerUp, sendTowerDown, sendTowerDownMidWay;

    void Start()
    {
        time = 0;
        startPos = this.transform.localPosition;
    }

    
    void Update()
    {
        if(sendTowerUp)
        {
            time += Time.deltaTime;
            Debug.Log("goes up");
            if(time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Slerp(startPos, endPosition, t);
            }
            else
            {
                time = 0;
                sendTowerUp = false;
            }
        }

        if (sendTowerDown)
        {
            time += Time.deltaTime;
            Debug.Log("goes down");
            if (time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Slerp(endPosition, startPos, t);
            }
            else
            {
                time = 0;
                sendTowerDown = false;
            }
        }

        if (sendTowerDownMidWay)
        {
            time += Time.deltaTime;
            Debug.Log("goes down midway");
            if (time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Slerp(midwayPos, startPos, t);
            }
            else
            {
                time = 0;
                sendTowerDown = false;
                sendTowerDownMidWay = false;
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            sendTowerUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (sendTowerUp) //still going up
            {
                sendTowerUp = false;
                time = 0;
                midwayPos = this.transform.localPosition;
                sendTowerDownMidWay = true;
            }
            else //already arrived on the top
            {
                sendTowerDown = true;
            }
        }
    }

    
}
