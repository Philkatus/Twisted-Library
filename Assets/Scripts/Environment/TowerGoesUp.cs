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

    
    void FixedUpdate()
    {
        if(sendTowerUp)
        {
            time += Time.fixedDeltaTime;
            if(time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Lerp(startPos, endPosition, t);
            }
            else
            {
                time = 0;
                sendTowerUp = false;
            }
        }

        if (sendTowerDown)
        {
            time += Time.fixedDeltaTime;
            if (time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Lerp(endPosition, startPos, t);
            }
            else
            {
                time = 0;
                sendTowerDown = false;
            }
        }

        if (sendTowerDownMidWay)
        {
            time += Time.fixedDeltaTime;
            if (time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Lerp(midwayPos, startPos, t);
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
        time = 0;
        if (other.gameObject.CompareTag("Player"))
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
                time = 0;
                sendTowerDown = true;
            }
        }
    }

    
}
