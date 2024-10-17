using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private CheckpointController controller;
    [SerializeField]
    private ParticleSystem particle;
    [SerializeField]
    private MeshRenderer flagLight;
    [SerializeField]
    private Material flagActiveM;
    [SerializeField]
    private MMF_Player feedback;

    private bool hasActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (controller.respawnPoint != transform && !hasActivated)
            {
                controller.respawnPoint = transform;
                feedback.PlayFeedbacks();

                if(flagLight != null)
                {
                    hasActivated = true;
                    flagLight.material = flagActiveM;
                }

                if (particle != null)
                {
                    particle.Play();
                }
            }
        }
    }
}
