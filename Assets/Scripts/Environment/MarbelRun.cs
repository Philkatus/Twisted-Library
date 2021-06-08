using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class MarbelRun : MonoBehaviour
{
    public bool finishIsOpen;


    [SerializeField] PathCreator rightPathCreator;
    [SerializeField] PathCreator wrongPathCreator;
    [SerializeField] GameObject marbel;
    [SerializeField] Transform Gate;
    VertexPath path;
    VertexPath rightPath;
    VertexPath wrongPath;
    [SerializeField] float rollingSpeed;
    float currentDistance;
    bool rolling;


    // Start is called before the first frame update
    void Start()
    {
        
        rightPath = rightPathCreator.path;
        wrongPath = wrongPathCreator.path;
        path = wrongPath;
        marbel.transform.position = path.GetPointAtDistance(0);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rolling)
        {
            ChoosePath();
            Move();
            Rotate();
        }

    }

    public void StartRolling() 
    {
        rolling = true;
        Debug.Log("startRolling");
    }

    public void OpenGate() 
    {
        finishIsOpen = true;
        Debug.Log("openGate");
    }

    public void ChoosePath() 
    {
        if (finishIsOpen&&currentDistance<path.GetClosestDistanceAlongPath(Gate.position))
        {
            path = rightPath;
        }
        
    }

    void Rotate() 
    {
        marbel.transform.forward = path.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);
        marbel.transform.Rotate(rollingSpeed*Mathf.PI*marbel.transform.localScale.x*Time.deltaTime, 0, 0,Space.Self);
    }

    void Move() 
    {
        currentDistance += rollingSpeed * Time.deltaTime;
        marbel.transform.position = path.GetPointAtDistance(currentDistance,EndOfPathInstruction.Stop);
        if (currentDistance >= path.length) 
        {
            if (path == wrongPath) 
            {
                marbel.transform.position = path.GetPointAtDistance(0);
                rolling = false;
            }
            else
            {
                Debug.Log("you did it");
            }
        }
    }
}