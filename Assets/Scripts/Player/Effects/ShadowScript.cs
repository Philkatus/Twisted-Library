using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowScript : MonoBehaviour
{
	[SerializeField] GameObject player;
	Vector3 offset;
    private void Start()
    {
        offset = transform.localPosition;
    }
    void Update()
	{
		transform.position = player.transform.position + offset;
	}
}
