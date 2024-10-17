using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    private Animator anim;
    [SerializeField]
    private GameObject explosionPrefab;
    [SerializeField]
    private float exploForce = 10f;
    [SerializeField]
    private CharacterController player;

    private List<Rigidbody> gotHit = new List<Rigidbody>();

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void StartExplision()
    {
        GetComponent<MeshRenderer>().enabled = true;
        anim.SetTrigger("Explode");
        GetComponentInParent<AudioSource>().Play();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 12);

        if(colliders.Length > 0 )
        {
            foreach(Collider collider in colliders)
            {
                var rigid = collider.GetComponent<Rigidbody>();
                if(rigid != null && rigid.gameObject != gameObject)
                {
                    if (!gotHit.Contains(rigid))
                    {
                        gotHit.Add(rigid);
                        //float dist = Vector3.Distance(rigid.transform.position, transform.position);

                        if (rigid.gameObject == player.gameObject)
                        {
                            if (!player.isBlocking)
                            {
                                player.TakeDamage();
                            }
                        }
                        else
                        {
                            var enemy = rigid.GetComponent<Enemy>();
                            var enemy2 = rigid.GetComponent<EnemyRanged>();
                            if (enemy != null)
                            {
                                enemy.Hit();
                            }
                            else if (enemy2 != null)
                            {
                                enemy2.Hit();
                            }
                        }
                        Vector3 dir = (rigid.transform.position - transform.position).normalized;
                        rigid.AddForce(dir * (exploForce), ForceMode.Impulse);
                    }
                }
            }
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(transform.parent.gameObject);
    }

}
