using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField]
    private CharacterController player;
    [SerializeField]
    private float hitForce = 100f;
    [SerializeField]
    private Transform exploPos;
    [SerializeField]
    private float exploForce = 100f;
    [SerializeField]
    private float exploRadius = 5f;

    private List<Rigidbody> gotHit = new List<Rigidbody>();
    private Animator anim;

    [SerializeField]
    private GameObject exploParticle;

    [Space(20)]
    [Header("Sounds")]
    [SerializeField]
    private AudioSource hit1;
    [SerializeField]
    private AudioSource hit2;
    [SerializeField]
    private AudioSource hit3;
    [Space(5)]
    [SerializeField]
    private MMF_Player contact;
    [SerializeField]
    private MMF_Player explodingHit;

    private bool canHit = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SwingDash()
    {
        player.SwingDash();
    }

    public void StartHit()
    {
        canHit = true;
        player.SwingStart();
    }

    public void EndHit()
    {
        canHit= false;
        gotHit.Clear();
        player.SwingEnd();
    }

    public void CanClick()
    {
        player.canClick = true;
    }

    private void HitSound1()
    {
        hit1.Play();
    }

    private void HitSound2()
    {
        hit2.Play();
    }

    private void HitSound3()
    {
        hit3.Play();
    }

    private float RandomPitch()
    {
        return Random.Range(-0.2f, 0.2f);
    }

    public void ExplodingHit()
    {
        Collider[] colliders = Physics.OverlapSphere(exploPos.position, exploRadius);

        foreach (Collider collider in colliders)
        {
            var rigid = collider.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                if (!gotHit.Contains(rigid))
                {
                    gotHit.Add(rigid);

                    //rigid.AddExplosionForce(exploForce, exploPos.position, exploRadius);
                    explodingHit.PlayFeedbacks();
                    GameObject part = Instantiate(exploParticle, exploPos.position, Quaternion.identity);

                    if(rigid.transform == player.transform)
                    {
                        Vector3 hitDir = ((rigid.transform.position - exploPos.position) + Vector3.up * 5).normalized;
                        rigid.AddForce(hitDir * exploForce, ForceMode.Impulse);
                    }
                    else
                    {
                        Vector3 hitDir = (rigid.transform.position - exploPos.position).normalized;
                        rigid.AddForce(hitDir * exploForce, ForceMode.Impulse);
                    }



                    var enemy = rigid.GetComponent<Enemy>();
                    var enemy2 = rigid.GetComponent<EnemyRanged>();
                    var barrel = rigid.GetComponentInChildren<Barrel>();

                    if (enemy != null)
                    {
                        enemy.Hit();
                    }
                    else if (enemy2 != null)
                    {
                        enemy2.Hit();
                    }
                    else if (barrel != null)
                    {
                        barrel.StartExplision();
                    }

                    var wig = rigid.GetComponentInChildren<WiggleHitPlayer>();
                    if (wig != null)
                    {
                        wig.Wiggle();
                    }
                }

            }
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (canHit)
        {
            var rigid = other.GetComponent<Rigidbody>();

            if (rigid != null)
            {
                if (rigid.tag != ("Player") && !gotHit.Contains(rigid))
                {
                    //GameObject part = Instantiate(hitParticle, exploPos.position, Quaternion.identity);
                    var enemy = other.GetComponent<Enemy>();
                    var enemy2 = rigid.GetComponent<EnemyRanged>();
                    var barrel = rigid.GetComponentInChildren<Barrel>();

                    if (enemy != null)
                    {
                        enemy.Hit();
                    }
                    else if (enemy2 != null)
                    {
                        enemy2.Hit();
                    }
                    else if (barrel != null)
                    {
                        barrel.StartExplision();
                    }

                    //Vector3 hitDir = rigid.transform.position - player.transform.position;
                    Vector3 hitDir = player.transform.forward;
                    hitDir.Normalize();
                    gotHit.Add(rigid);
                    rigid.AddForce(hitDir * hitForce, ForceMode.Impulse);

                    var wig = rigid.GetComponentInChildren<WiggleHitPlayer>();
                    if (wig != null)
                    {
                        wig.Wiggle();
                    }
                    contact.PlayFeedbacks();
                }
            }
        }
    }

}
