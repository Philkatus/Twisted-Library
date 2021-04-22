using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class DataScriptableObject : ScriptableObject
{
    public float jumpHeight;

    [SerializeField]
    private float speed;

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            Debug.Log(value);

            speed = value;
        }
    }
}
