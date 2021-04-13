using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    public virtual IEnumerator initialize()
    {
        yield return null;
    }

    public virtual IEnumerator Movement()
    {
        yield return null;
    }

    public virtual IEnumerator Jump()
    {
        yield return null;
    }

    public virtual IEnumerator Snap() 
    {
        yield return null;
    }

    public virtual IEnumerator GroundDetection() 
    {
        yield return null;
    }

    public virtual IEnumerator Finish() 
    {
        yield return null;
    }
}
