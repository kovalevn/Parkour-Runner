using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (player.playerState == PlayerState.Airborne || player.playerState == PlayerState.Falling)
        {
            player.playerState = PlayerState.Landing;
        }
    }
}
