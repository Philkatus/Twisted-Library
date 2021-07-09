using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicRailVolcano : MonoBehaviour
{
    [SerializeField] Vector3 endPosition, planeEndPos;
    [SerializeField] float travelTime;
    [SerializeField] GameObject bigPlate;
    public bool noPlane;
    PlayerMovementStateMachine psm;
    bool done;
    Vector3 startingPos, planeStartPos;

    private void Start()
    {
        startingPos = transform.localPosition;
        planeStartPos = bigPlate.transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && !done)
        {
            psm = other.GetComponent<PlayerMovementStateMachine>();
            if (psm.playerState == PlayerMovementStateMachine.PlayerState.swinging)
            {
                StartCoroutine(MoveDown());
                done = true;
            }
        }
    }
    IEnumerator MoveUp(Vector3 currPos)
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        if (!noPlane)
            StartCoroutine(MovePlateAway());
        float timer = 0;
        while (timer <= travelTime)
        {
            float t = timer / travelTime;
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(currPos, endPosition, t);
            yield return delay;
        }
        psm.Jump();
        while (psm.playerState == PlayerMovementStateMachine.PlayerState.swinging)
        {
            yield return delay;
        }
        StartCoroutine(ShortMoveUp());
        if (!noPlane)
            StartCoroutine(MovePlateBack());
    }
    IEnumerator MoveDown()
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();

        float timer = 0;
        while (timer <= 0.5f)
        {
            float t = timer / 0.5f;
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startingPos, startingPos + Vector3.up * 1, t);
            yield return delay;
        }

        StartCoroutine(MoveUp(transform.localPosition));
    }
    IEnumerator MoveBackDown(Vector3 currPos)
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        float timer = 0;
        while (timer <= travelTime / 2.5f)
        {
            float t = timer / (travelTime / 2.5f);
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(endPosition, startingPos, t);
            yield return delay;
        }
        done = false;
    }
    IEnumerator ShortMoveUp()
    {
        float timer = 0;
        while (timer <= 0.5f)
        {
            float t = timer / 0.5f;
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(endPosition, endPosition + Vector3.up * 1, t);
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(MoveBackDown(transform.position));
    }
    IEnumerator MovePlateAway()
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        float timer = 0;
        while (timer <= travelTime / 2)
        {
            float t = timer / (travelTime * 0.5f);
            timer += Time.deltaTime;
            bigPlate.transform.localPosition = Vector3.Lerp(planeStartPos, planeEndPos, t);
            yield return delay;
        }
    }
    IEnumerator MovePlateBack()
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        float timer = 0;
        while (timer <= travelTime / 2)
        {
            float t = timer / (travelTime * 0.5f);
            timer += Time.deltaTime;
            bigPlate.transform.localPosition = Vector3.Lerp(planeEndPos, planeStartPos, t);
            yield return delay;
        }
    }
}
