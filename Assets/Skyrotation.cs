using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class Skyrotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] Volume volume;

    private HDRISky sky;


    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet<HDRISky>(out sky);
    }

    // Update is called once per frame
    void Update()
    {
        if (sky != null)
        {
            var temp = sky.rotation.value + 1;
            if(temp >= 360){
                temp = 0;
            }
            sky.rotation.value = Mathf.Lerp(sky.rotation.value, temp, Time.deltaTime * (rotationSpeed/10));
        }
    }


}
