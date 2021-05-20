using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ShadowScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    PlayerMovementStateMachine pSM;
    DecalProjector projector;
    Vector3 offset;
    private void Start()
    {
        offset = transform.position - player.transform.position;
        pSM = player.GetComponent<PlayerMovementStateMachine>();
        projector = this.GetComponent<DecalProjector>();
    }
    void Update()
    {
        transform.position = player.transform.position + offset;
        if (pSM.playerState == PlayerMovementStateMachine.PlayerState.inTheAir || pSM.playerState == PlayerMovementStateMachine.PlayerState.walking)
        {
            projector.enabled = true;
        }
        else
        {
            projector.enabled = false;
        }
    }
}
