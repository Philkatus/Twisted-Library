using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Prototype_Respawn_script : MonoBehaviour
{
    [SerializeField] private Transform player, respawnPoint;
    [SerializeField] private bool isRestartWithKey;
    public InputActionAsset actionAsset;
    InputActionMap playerControlsMap;
    InputAction restartButton;

    private void Start()
    {
        playerControlsMap = actionAsset.FindActionMap("PlayerControls");
        playerControlsMap.Enable();
        restartButton = playerControlsMap.FindAction("Restart");

        //restartButton.performed += context => RespawnPlayer();
    }
    private void OnTriggerEnter(Collider other)
    {
        RespawnPlayer();
    }

    private void Update()
    {
        if(restartButton.triggered && isRestartWithKey)
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        player.GetComponentInChildren<CharacterController>().enabled = false;
        player.transform.position = respawnPoint.transform.position;
        player.GetComponentInChildren<CharacterController>().enabled = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(respawnPoint.position, 0.5f);
    }
}
