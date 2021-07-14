using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGoesUp : MonoBehaviour
{

    [SerializeField] Vector3 endPosition;
    [SerializeField] float travelTime;
    float time;
    Vector3 startPos, midwayPos;

    bool sendTowerUp, sendTowerDown, sendTowerDownMidWay, sendTowerUpMidWay, moving;

    void Start()
    {
        AudioManager.Instance.MovingLandmarkOneColumn = this.GetComponent<ResonanceAudioSource>();
        time = 0;
        startPos = this.transform.localPosition;
    }


    void FixedUpdate()
    {
        if (sendTowerUp)
        {
            time += Time.fixedDeltaTime;
            if (time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Lerp(startPos, endPosition, t);
            }
            else
            {
                time = 0;
                sendTowerUp = false;
                moving = false;
                AudioManager.Instance.StopColumnSound(transform.position);
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
                moving = false;
                AudioManager.Instance.StopColumnSound(transform.position);
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
                moving = false;
                AudioManager.Instance.StopColumnSound(transform.position);
            }
        }

        if (sendTowerUpMidWay)
        {
            time += Time.fixedDeltaTime;
            if (time < travelTime)
            {
                float t = time / travelTime;
                transform.localPosition = Vector3.Lerp(midwayPos, endPosition, t);
            }
            else
            {
                time = 0;
                sendTowerUp = false;
                sendTowerUpMidWay = false;
                moving = false;
                AudioManager.Instance.StopColumnSound(transform.position);
            }
        }

        if (!moving && (sendTowerDown || sendTowerDownMidWay || sendTowerUp || sendTowerUpMidWay))
        {
            moving = true;
            AudioManager.Instance.ColumnSound(transform.position);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (sendTowerDownMidWay || sendTowerDown) //still going down
            {
                sendTowerDown = false;
                sendTowerDownMidWay = false;
                time = 0;
                midwayPos = this.transform.localPosition;
                sendTowerUpMidWay = true;
            }
            else // already down
            {
                time = 0;
                sendTowerUp = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (sendTowerUp || sendTowerUpMidWay) //still going up
            {
                sendTowerUp = false;
                sendTowerUpMidWay = false;
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
