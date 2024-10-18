using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    [SerializeField]
    GameController gameController;
    [Space(20)]
    public Transform respawnPoint;
    [SerializeField]
    private CameraHandler cameraHandler;
    [SerializeField]
    private CharacterController player;

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Sword")
        {
            player.rb.linearVelocity = Vector3.zero;
            other.transform.position = respawnPoint.position;
            other.transform.rotation = respawnPoint.rotation;
            cameraHandler.lookAngle = respawnPoint.eulerAngles.y;
            player.OutOfBoundsDamage();
        }
        else if(other.tag == "Enemy" || other.tag == "Enemy2")
        {
            gameController.GetCrystal();
            Destroy(other.gameObject);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

}
