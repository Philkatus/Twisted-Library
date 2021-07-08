using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleStuffEasilyDude : MonoBehaviour
{
    [SerializeField] float scaleSpeed, endScale;
    public bool isPlaying;



    // Start is called before the first frame update
    void Start()
    {
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            if(this.transform.localScale.y >= endScale)
            {
                this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y - (scaleSpeed * Time.deltaTime), this.transform.localScale.z);
            }
            else
            {
                isPlaying = false;
            }
        }
    }
}
