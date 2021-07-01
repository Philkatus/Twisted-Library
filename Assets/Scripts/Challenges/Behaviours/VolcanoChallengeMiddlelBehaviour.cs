using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoChallengeMiddlelBehaviour : MonoBehaviour, ICentralObject
{

    [SerializeField] Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAllComponentsCompleted()
    {
        animator.SetBool("isActivated", true);
    }
}
