using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldManager : MonoBehaviour
{
    [SerializeField]
    private float blockForce = 500f;
    [SerializeField]
    private CharacterController player;
    private bool shieldActive = false;
    [SerializeField]
    private MMF_Player shieldFeedback;
    [SerializeField]
    private MMF_Player blockFeedback;

    private List<Rigidbody> booped = new List<Rigidbody>();

    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = true;   
    }

    public void ShieldActivate()
    {
        shieldFeedback.PlayFeedbacks();
        shieldActive = true;
        player.isBlocking = true;
    }

    public void ShieldDeActivate()
    {
        shieldActive = false;
        player.isBlocking = false;
        booped.Clear();
    }


    private void OnTriggerStay(Collider other)
    {
        if (shieldActive)
        {
            var obj = other.GetComponent<Rigidbody>();
            var missile = other.GetComponent<Missile>();
            if(missile != null && !booped.Contains(obj))
            {
                booped.Add(obj);
                missile.SetTarget(missile.enemy);
                missile.transform.forward = (-missile.transform.forward + Vector3.up).normalized;
                missile.elapsedTime = 0f;
                missile.GetComponent<MeshRenderer>().material = missile.blockedMaterial;
                missile.GetComponentInChildren<TrailRenderer>().material = missile.blockedTrailMaterial;
                blockFeedback.PlayFeedbacks();
            }
            else if (obj != null && obj.tag != "Player" && !booped.Contains(obj))
            {
                booped.Add(obj);
                var enemy = obj.GetComponent<Enemy>();
                if(enemy != null)
                {
                    if (enemy.isHitting)
                    {
                        enemy.Hit();
                    }
                }

                Vector3 forceDir = obj.transform.position - transform.position;
                forceDir.Normalize();

                blockFeedback.PlayFeedbacks();
                obj.AddForce(forceDir * blockForce, ForceMode.Impulse);

            }
        }
    }

}
