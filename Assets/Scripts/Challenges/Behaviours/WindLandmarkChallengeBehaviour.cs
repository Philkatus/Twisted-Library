using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindLandmarkChallengeBehaviour : MonoBehaviour
{
    [SerializeField] List<Animation> anims;

    public bool play;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var anim in anims)
        {
            anim.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug
        if (play)
        {
            foreach (var anim in anims)
            {
                anim.Play();
            }

            play = false;
        }
    }

    public void OnAllComponentsCompleted()
    {
        foreach (var anim in anims)
        {
            anim.Play();
        }
    }
}
