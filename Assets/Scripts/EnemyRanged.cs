using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using MoreMountains.Feedbacks;

public class EnemyRanged : MonoBehaviour
{
    [SerializeField]
    GameController gameController;
    [SerializeField]
    private GameObject meshes;

    [Space(20)]
    [Header("Ai Targeting")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float targetDist = 20f;
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

    private bool isAggroed = false;

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

    [Space(20)]
    [Header("Ai Stats")]
    [SerializeField]
    private int HP = 2;
    [SerializeField]
    private float attackRange = 10f;
    [SerializeField]
    private float attackCooldown = 5f;
    [SerializeField]
    private GameObject attackIndicator;
    [SerializeField]
    private GameObject missile;
    [SerializeField]
    Transform missileTransform;
    [SerializeField]
    private GameObject missileParticle;


    private bool isOnAttackCooldown = false;
    public bool isAttacking = false;
    private bool gotHit;

    private Vector3 idlePos;
    private Vector3 targetPosition;
    public Path path;
    private int currentWaypoint;

    private Vector3 gizmoDir;

    private Seeker seeker;
    private Rigidbody rb;
    private RVOController RVO;

    private Vector3 delta;
    private bool isDead;

    [SerializeField]
    private ParticleSystem diePrefab;
    [SerializeField]
    private MMF_Player attackFeedback;
    [SerializeField]
    private MMF_Player gotHitFeedback;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody>();
        seeker.pathCallback += OnPathComplete;
        idlePos = transform.position;
        idleDelay = Random.Range(idleDelay / 2, idleDelay * 2);
        RVO = GetComponent<RVOController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDead)
        {
            Attacking();
            GetPath();
            GetDelta();
            Turning();
        }

    }

    private void FixedUpdate()
    {
        if (isOnAttackCooldown && !gotHit && !isDead)
        {
            Walk();
        }
        else if (!gotHit && !isAttacking && Vector3.Distance(transform.position, target.position) > attackRange)
        {
            Walk();
        }
    }

    private void GetPath()
    {
        float dist = Vector3.Distance(transform.position, target.position);

        if (isOnAttackCooldown && canAttack)
        {
            CooldownPath();
            isAggroed = true;
        }
        else if(canAttack && dist < targetDist)
        {
            AttackPath();
            isAggroed = true;
        }
        else 
        {
            IdlePath();
            isAggroed = false;
        }
    }

    private void IdlePath()
    {
        GetIdlePath();

        if (path == null)
        {
            return;
        }

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

                    break;
                }
            }
            else
            {
                break;
            }

        }
    }

    private void CooldownPath()
    {
        idleDelayTimer = 0;

        reIntervalTimer += Time.deltaTime;

        if (reIntervalTimer >= pathReUpdateInterval * 2.8f)
        {
            reIntervalTimer = 0f;

            Vector3 dirAway = (transform.position - target.position).normalized * (attackRange - 1);

            seeker.StartPath(transform.position, dirAway);
        }

        if (path == null)
        {
            return;
        }

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

                    break;
                }
            }
            else
            {
                break;
            }

        }
    }

    private void GetDelta()
    {
        if (path == null) { return; }
        RVO.SetTarget(path.vectorPath[currentWaypoint], 50, 100);
        delta = RVO.CalculateMovementDelta(transform.position, Time.deltaTime);
    }

    private void Attacking()
    {
        if (Vector3.Distance(transform.position, target.position) < attackRange && !isAttacking && !isOnAttackCooldown && !gotHit && canAttack)
        {
            isAttacking = true;
            StartCoroutine(Attack());
        }

        if (isAttacking)
        {
            attackIndicator.SetActive(true);
        }
        else
        {
            attackIndicator.SetActive(false);
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f * 2.8f);
        if (!gotHit)
        {
            attackFeedback.PlayFeedbacks();
            isOnAttackCooldown = true;
            isAttacking = false;
            path = null;
            Vector3 attackDir = (transform.forward + (target.position - transform.position)).normalized;
            rb.AddForce(-attackDir * moveForce / 3, ForceMode.Impulse);
            var mis = Instantiate(missile, missileTransform.position, missileTransform.rotation);
            var buh = mis.GetComponent<Missile>();
            buh.SetTarget(target);
            buh.enemy = transform;
            Instantiate(missileParticle, missileTransform.position, missileTransform.rotation);
            StartCoroutine(AttackCooldown());
        }
        else
        {
            isAttacking = false;
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown * 2.8f);
        isOnAttackCooldown = false;
    }

    private void Walk()
    {
        if (path == null)
        {
            return;
        }

        Vector3 rbVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        Vector3 moveDir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        moveDir.y = 0f;

        float distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);

        moveDir = delta * Time.deltaTime;

        if (moveDir.normalized.magnitude <= 1 && Vector3.Dot(moveDir, rbVel) > 0.2f)
        {
            moveDir = -Vector3.Reflect(rbVel, moveDir);
        }

        moveDir.Normalize();

        //get vel based on dir
        float dirVel = Vector3.Dot(rbVel, moveDir);

        gizmoDir = moveDir;

        if (dirVel < maxSpeed && distanceToWaypoint > 2f && isAggroed)
        {
            rb.AddForce(moveDir * moveForce);
        }
        else if (dirVel < maxSpeed / 2 && distanceToWaypoint > 2f)
        {
            rb.AddForce(moveDir * moveForce);
        }

    }

    private void Turning()
    {
        if (path == null)
        {
            return;
        }

        Vector3 moveDir;

        if (!isAggroed)
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
                float minRange = -3;
                float maxRange = 3;
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

        if (reIntervalTimer >= pathReUpdateInterval * 2.8f)
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
        targetPosition = Vector3.zero;
        path = null;
    }

    public void CannotAttack()
    {
        canAttack = false;
        path = null;
        idleDelayTimer = 0f;
    }

    public void Hit()
    {
        gotHit = true;
        isAttacking = false;
        HP--;
        gotHitFeedback.PlayFeedbacks();
        if (HP <= 0)
        {
            isDead = true;
            Instantiate(diePrefab, transform.position, Quaternion.identity);
            gameController.GetCrystal();
            attackIndicator.SetActive(false);
            Destroy(gameObject, 5);
            GetComponent<BoxCollider>().enabled = false;
            meshes.SetActive(false);
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
        StartCoroutine(GotHitCooldown());
    }

    private IEnumerator GotHitCooldown()
    {
        yield return new WaitForSeconds(0.5f * 2.8f);
        gotHit = false;
    }

    private void OnDisable()
    {
        seeker.pathCallback -= OnPathComplete;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, gizmoDir + transform.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetDist);
    }
}
