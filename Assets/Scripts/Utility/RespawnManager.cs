using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    public enum RespawnMode 
    {
        samePosition,
        Start,
        LastGroundPosition,
        LastCheckPoint

    }
    private void Awake()
    {
        if(Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name,LoadSceneMode.Single);
    }

    public void Respawn(RespawnMode respawnMode) 
    {
        switch (respawnMode) 
        {
            case RespawnMode.samePosition:
                break;
            case RespawnMode.Start:
                break;
            case RespawnMode.LastCheckPoint:
                break;
            case RespawnMode.LastGroundPosition:
                break;
        }
    }

}
