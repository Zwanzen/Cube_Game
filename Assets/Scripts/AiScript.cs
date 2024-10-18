using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiScript : MonoBehaviour
{

    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private float moveForce;
    [SerializeField]
    private float followDist;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 moveDir = targetTransform.position - transform.position;
        //moveDir.y = 0;

        Vector3 rbVel = rb.linearVelocity;


        if (moveDir.normalized.magnitude <= 1 && Vector3.Dot(moveDir, rbVel) > 0.2f)
        {
            moveDir = -Vector3.Reflect(rbVel, moveDir);
        }

        moveDir.Normalize();


        if (Vector3.Distance(transform.position, targetTransform.position) < followDist)
        {
            rb.AddForce(moveDir * moveForce);
        }

    }


}
