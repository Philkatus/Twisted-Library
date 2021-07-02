using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoChallengeEntryBehaviour : MonoBehaviour, ICentralObject
{
    [SerializeField] List<Animation> anims;

    [SerializeField] VolcanoGeneralBehaviour volcanoBehaviour;

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
        if(play)
        {
            foreach (var anim in anims)
            {
                anim.Play();
            }

            //should opne the first first, else we can't see it
            if (!volcanoBehaviour.upPlateOut)
            {
                volcanoBehaviour.plateGate[0].Play();
                volcanoBehaviour.upPlateOut = true;
            }
            else
            {
                if (!volcanoBehaviour.middlePlateOut)
                {
                    volcanoBehaviour.plateGate[1].Play();
                    volcanoBehaviour.middlePlateOut = true;

                }
                else
                {
                    if (!volcanoBehaviour.downPlateOut)
                    {
                        volcanoBehaviour.plateGate[2].Play();
                        volcanoBehaviour.downPlateOut = true;
                    }
                }
            }

            play = false;
        }
    }

    public void OnAllComponentsCompleted()
    {
        foreach(var anim in anims)
        {
            anim.Play();
        }


        //should open the first first, else we can't see it
        if (!volcanoBehaviour.upPlateOut)
        {
            volcanoBehaviour.plateGate[0].Play();
            volcanoBehaviour.upPlateOut = true;
        }
        else
        {
            if (!volcanoBehaviour.middlePlateOut)
            {
                volcanoBehaviour.plateGate[1].Play();
                volcanoBehaviour.middlePlateOut = true;

            }
            else
            {
                if (!volcanoBehaviour.downPlateOut)
                {
                    volcanoBehaviour.plateGate[2].Play();
                    volcanoBehaviour.downPlateOut = true;
                }
            }
        }
    }
}
