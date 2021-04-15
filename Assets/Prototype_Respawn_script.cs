using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prototype_Respawn_script : MonoBehaviour
{
    [SerializeField] private Transform player, respawnPoint;
    private void OnTriggerEnter(Collider other)
    {
        player.transform.position = respawnPoint.transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(respawnPoint.position, 0.5f);
    }
}
