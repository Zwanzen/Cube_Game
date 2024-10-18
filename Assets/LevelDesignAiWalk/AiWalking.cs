using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Threading;

public class AiWalking : MonoBehaviour
{
    [Header("Ai Targeting")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float targetDist = 10f;
    [SerializeField]
    private bool pathDebug;
    [SerializeField]
    private float pathUpdateInterval = 0.1f;
    [SerializeField]
    private float pathReUpdateInterval = 1f;

    private float intervalTimer = 0f;
    private float reIntervalTimer = 0f;

    private bool canAttack = false;
    private float idleDelay = 2f;
    private float idleDelayTimer = 0f;

    private bool isAttacking = false;

    [Space(20)]
    [Header("Ai Movement")]
    [SerializeField]
    private float moveForce = 8f;
    [SerializeField]
    private float maxSpeed = 15f;
    [SerializeField]
    private float nextWaypointDistance = 3;
    [SerializeField]
    private float rotationSpeed = 100f;

    private Vector3 idlePos;
    private Vector3 targetPosition;
    public Path path;
    private int currentWaypoint;
    private bool reachedEndOfPath;

    private Vector3 gizmoDir;

    private Seeker seeker;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody>();
        seeker.pathCallback += OnPathComplete;
        idlePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        GetPath();
        Turning();
    }

    private void FixedUpdate()
    {
        Walk();
    }

    private void GetPath()
    {
        float dist = Vector3.Distance(transform.position, target.position);

        if (!canAttack || dist > targetDist)
        {
            IdlePath();
            isAttacking = false;
        }
        else
        {
            AttackPath();
            isAttacking = true;
        }
    }

    private void IdlePath()
    {
        GetIdlePath();

        if (path == null)
        {
            return;
        }

        reachedEndOfPath = false;
        float distanceToWaypoint;

        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }

        }
    }

    private void AttackPath()
    {
        idleDelayTimer = 0;

        if (targetPosition != target.position)
        {
            PathUpdateInterval();
            reIntervalTimer = 0f;
        }
        else
        {
            PathReUpdateOnInterval();
            intervalTimer = 0f;
        }

        if (path == null)
        {
            return;
        }

        reachedEndOfPath = false;
        float distanceToWaypoint;

        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }

        }
    }

    private void Walk()
    {
        if(path == null)
        {
            return;
        }

        Vector3 rbVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        Vector3 moveDir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        moveDir.y = 0f;
        gizmoDir = moveDir;

        if (moveDir.normalized.magnitude <= 1 && Vector3.Dot(moveDir, rbVel) > 0.2f)
        {
            moveDir = -Vector3.Reflect(rbVel, moveDir);
        }

        //normalize before force
        moveDir.Normalize();

        //get vel based on dir
        float dirVel = Vector3.Dot(rbVel, moveDir);

        float distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);


        if (dirVel < maxSpeed && distanceToWaypoint > 2f)
        {
            rb.AddForce(moveDir * moveForce);
        }
    }

    private void Turning()
    {
        Vector3 moveDir;

        if (!isAttacking)
        {
            moveDir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            moveDir.y = 0f;

            if (moveDir != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            moveDir = (target.position - transform.position).normalized;
            moveDir.y = 0f;

            if (moveDir != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }



    }

    private void GetIdlePath()
    {
        idleDelayTimer += Time.deltaTime;

        if (idleDelayTimer >= idleDelay * 2.8f)
        {
            idleDelayTimer = 0f;

            if (Vector3.Distance(transform.position, idlePos) > 4f)
            {
                seeker.StartPath(transform.position, idlePos);
            }
            else
            {
                float minRange = -2;
                float maxRange = 2;
                Vector3 random = new Vector3(Random.Range(minRange, maxRange), 0, Random.Range(minRange, maxRange));
                seeker.StartPath(idlePos, idlePos + random);
            }
        }
    }

    private void PathUpdateInterval()
    {
        intervalTimer += Time.deltaTime;

        if (intervalTimer >= pathUpdateInterval * 2.8f)
        {
            intervalTimer = 0f;

            targetPosition = target.position;
            seeker.StartPath(transform.position, targetPosition);
        }
    }

    private void PathReUpdateOnInterval()
    {
        reIntervalTimer += Time.deltaTime;

        if(reIntervalTimer >= pathReUpdateInterval * 2.8f)
        {
            reIntervalTimer = 0f;

            targetPosition = target.position;
            seeker.StartPath(transform.position, targetPosition);
        }
    }

    public void OnPathComplete(Path p)
    {
        if (pathDebug)
        {
            Debug.Log("A path was calculated. Did it fail with an error? " + p.error);
        }

        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
    }

    public void CanAttack()
    {
        canAttack = true;
    }

    public void CannotAttack()
    {
        canAttack = false;
        path = null;
        idleDelayTimer = 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, gizmoDir + transform.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetDist);
    }
}
