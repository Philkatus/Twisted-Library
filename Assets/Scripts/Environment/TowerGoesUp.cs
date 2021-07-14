using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGoesUp : MonoBehaviour
{

    [SerializeField] Vector3 endPosition;
    [SerializeField] float travelTime;
    float time;
    Vector3 startPos, midwayPos;
    PlayerMovementStateMachine psm;

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
                AudioManager.Instance.StopColumnSound();
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
                AudioManager.Instance.StopColumnSound();
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
                AudioManager.Instance.StopColumnSound();
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
                AudioManager.Instance.StopColumnSound();
            }
        }

        if (!moving && (sendTowerDown || sendTowerDownMidWay || sendTowerUp || sendTowerUpMidWay))
        {
            moving = true;
            AudioManager.Instance.LandmarkOneColumnSound();
        }
    }

    public bool changedStateToSwinging, changedStateToinTheAir, firstSnapDoneSinceInTriggerCollider;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            psm = other.GetComponent<PlayerMovementStateMachine>();
            if (psm.playerState == PlayerMovementStateMachine.PlayerState.swinging && !changedStateToSwinging)
            {
                changedStateToSwinging = true;
                changedStateToinTheAir = false;

                firstSnapDoneSinceInTriggerCollider = true;

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
            else if (psm.playerState == PlayerMovementStateMachine.PlayerState.inTheAir && !changedStateToinTheAir && firstSnapDoneSinceInTriggerCollider)
            {
                changedStateToinTheAir = true;
                changedStateToSwinging = false;

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



    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            firstSnapDoneSinceInTriggerCollider = false; //set back to false

            if(this.transform.localPosition.y <= endPosition.y + 0.1f &&
                this.transform.localPosition.y >= endPosition.y - 0.1f)
            {
                sendTowerDown = true; // when sliding out
                changedStateToSwinging = false;
            }
        }

    }


}
