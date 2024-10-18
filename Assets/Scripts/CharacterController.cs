using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private Transform victoryCam;
    [SerializeField]
    SaveSettings saveSettings;

    [Space(20)]
    [Header("Movement Variables")]
    [SerializeField]
    private float moveForce = 5f;
    [SerializeField]
    private float maxSpeed = 10f;
    [SerializeField]
    private float rotationSpeed = 720;
    [SerializeField]
    private float jumpForce = 10f;
    [SerializeField]
    float jumpDelay = 0.2f;
    [SerializeField]
    float jumpLeway = 1f;

    private bool jumping;
    private bool canJump;


    [SerializeField]
    private float dashForce = 1f;
    [SerializeField]
    private float dashCooldown = 2f;
    [SerializeField]
    private Image dashImage;


    private bool canDash = true;

    [Space(20)]
    [Header("Sword Variables")]
    [SerializeField]
    private GameObject SwordObject;
    [SerializeField]
    private Animator swordAnim;
    [SerializeField]
    private float swordCooldownTime = 2f;
    [SerializeField]
    private float swingDashForce = 5f;

    private bool hasSword = false;

    private float nextFireTime = 0f;
    private static int noOfClicks = 0;
    private float lastClickedTime = 0f;
    private float maxComboDelay = 1f * 2.8f;
    private bool isSwinging;

    public bool canClick = true;

    private Vector3 swingDir;
    private float swordRotTimer;


    [Space(20)]
    [Header("Shield Variables")]
    [SerializeField]
    private GameObject ShieldObject;
    [SerializeField]
    private MeshRenderer button;
    [SerializeField]
    private Material buttonColorRed;
    [SerializeField]
    private Material buttonColorGreen;
    [SerializeField]
    private float shieldCooldown = 1.5f;
    [SerializeField]
    private Animator shieldAnim;

    private bool hasShield = false;
    private bool shieldReady = true;

    [HideInInspector]
    public bool isBlocking = false;


    [Space(20)]
    [Header("Grapple Variables")]
    [SerializeField]
    private GameObject grappleObject;
    [SerializeField]
    private float grappleLenght = 1f;
    [SerializeField]
    private float grappleRadius = 1f;
    [SerializeField]
    private LayerMask grappleLayer;
    [SerializeField]
    private Transform grappleLRpoint;
    [SerializeField]
    private Transform grappleGun;

    private bool hasGrapple = false;

    private SpringJoint joint;
    [HideInInspector]
    public bool isGrappeling;

    [HideInInspector]
    public Transform grappleTransform;
    private float grappleDistance;

    private Transform preGrapple;
    private float preGrappleDistance;

    private Vector3 currentGrapplePosition;

    [Space(20)]
    [Header("Feedbacks")]
    [SerializeField]
    private MMF_Player landFeedback;
    [SerializeField]
    private MMF_Player dashFeedback;
    [SerializeField]
    private MMF_Player jumpFeedback;
    [SerializeField]
    private MMF_Player takeDamageFeedback;
    [SerializeField]
    private MMF_Player powerupFeedback;
    [SerializeField]
    private MMF_Player grappleFeedback;

    private MMF_Scale scaleFeedback;

    [Space(20)]
    [Header("Particle Handling")]
    [SerializeField]
    private ParticleSystem walkingParticle;

    private bool landed;

    [Space(20)]
    [Header("Ground Check")]
    [SerializeField]
    private Vector3 checkerCollider;
    [SerializeField]
    private Vector3 checkerPosition;
    [SerializeField]
    private LayerMask groundLayer;


    //walljump
    private bool isWalljump;
    private Vector3 wallJumpDir;
    private float wallDur;
    private int wallJumps = 0;
    [SerializeField]
    private LayerMask wallJumpLayer;

    [Space(20)]
    [Header("Gizmo")]
    [SerializeField]
    private float gizmoStreanght = 1f;
    [SerializeField]
    private bool isGizmo = true;
    [SerializeField]
    private bool isGizmo2 = true;
    [SerializeField]
    private bool isGizmo3 = true;

    private float elapsedTime = 0f;

    private float dirVel;
    private Vector3 dir;

    [HideInInspector]
    public Rigidbody rb;
    public Transform cameraHolder;
    [SerializeField]
    private Transform mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        scaleFeedback = powerupFeedback.GetFeedbackOfType<MMF_Scale>();
    }

    private void Update()
    {
        ParticleHandler();
        ShieldHandler();

        Turning();

        GrappleHandler();
        Dash();


        if(!isGrappeling)
        {
            SwordHandler();
        }
        else
        {
            noOfClicks = 0;
        }

        elapsedTime += Time.deltaTime;
    }

    private void LateUpdate()
    {
        WallJumpChecker();  
        Jump();
    }

    private void FixedUpdate()
    {
        if (!isSwinging)
        {
            Walking();
        }
    }

    private Vector3 MoveVector()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        return new Vector3(moveX, 0, moveZ);
    }

    private void Walking()
    {
        //get dir
        Vector3 moveDir = MoveVector();
        //get vel
        Vector3 rbVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        //orientate based on camera
        //moveDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * moveDir;
        if (!gameController.victory)
        {
            moveDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * moveDir;
        }
        else
        {
            moveDir = Quaternion.AngleAxis(victoryCam.rotation.eulerAngles.y, Vector3.up) * moveDir;
        }

        //if move, counter vel for better control
        if (MoveVector().normalized.magnitude <= 1 && Vector3.Dot(moveDir, rbVel) > 0.2f && !isGrappeling)
        {
            moveDir = -Vector3.Reflect(rbVel, moveDir);
        }
        
        //normalize before force
        moveDir.Normalize();

        //get vel based on dir
        dirVel = Vector3.Dot(rbVel, moveDir);

        //ADD COUNTER FORCE IF BHOP---------------------------------------------------------------------------------Later

        //Project onto normal
        moveDir = Vector3.ProjectOnPlane(moveDir, GetSurfaceNormal());

        dir = moveDir;

        //decide if i can apply force in air vs grounded
        if (dirVel < maxSpeed && !IsGrounded())
        {
            rb.AddForce(moveDir * moveForce/3);
        }
        else if (dirVel < maxSpeed)
        {
            rb.AddForce(moveDir * moveForce);
        }
    }

    private void Turning()
    {
        swordRotTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(swordRotTimer / (0.5f * 2.8f));
        float exponentialProgress = Mathf.Pow(progress, 5f);
        float delayedRot = Mathf.Lerp(0, rotationSpeed, exponentialProgress);

        if (isSwinging)
        {
            /*
            Quaternion toRotation = Quaternion.LookRotation(swingDir, GetSurfaceNormal());

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * 8 * Time.deltaTime);
            */
        }
        else 
        {
            Vector3 moveDir = MoveVector().normalized;
            if (!gameController.victory)
            {
                moveDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * moveDir;
            }
            else
            {
                moveDir = Quaternion.AngleAxis(victoryCam.rotation.eulerAngles.y, Vector3.up) * moveDir;
            }

            //turning
            if (isGrappeling && moveDir != Vector3.zero)
            {
                Vector3 grappleUp = grappleTransform.position - transform.position;
                grappleUp.Normalize();

                Vector3 projectedDir = Vector3.ProjectOnPlane(moveDir, grappleUp);

                Quaternion toRotation = Quaternion.LookRotation(projectedDir, grappleUp);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
            else if (isGrappeling)
            {
                Vector3 grappleUp = grappleTransform.position - transform.position;
                grappleUp.Normalize();

                Quaternion toRotation = Quaternion.FromToRotation(transform.up, grappleUp) * transform.rotation;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
            else if (dir != Vector3.zero)
            {

                Quaternion toRotation = Quaternion.LookRotation(dir, GetSurfaceNormal());

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, delayedRot * Time.deltaTime);
            }
            else
            {
                Quaternion toRotation = Quaternion.FromToRotation(transform.up, GetSurfaceNormal()) * transform.rotation;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, delayedRot * Time.deltaTime);
            }
        }
    }

    private void Dash()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            canDash = false;
            dashImage.fillAmount = 1;

            Vector3 dashDir = MoveVector().normalized;

            if (dashDir != Vector3.zero)
            {

                dashDir = Quaternion.AngleAxis(cameraHolder.rotation.eulerAngles.y, Vector3.up) * dashDir;
                dashDir = new Vector3(dashDir.x, 0, dashDir.z);
            }
            else
            {
                dashDir = Quaternion.Euler(0, cameraHolder.rotation.eulerAngles.y, 0) * Vector3.forward;
                dashDir.Normalize();
            }

            if(IsGrounded()) 
            {
                if (isSwinging)
                {
                    rb.AddForce(dashDir * dashForce * 1.1f, ForceMode.Impulse);
                }
                else
                {
                    rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
                }
            }
            else
            {
                if(isSwinging)
                {
                    rb.AddForce(dashDir * dashForce * 0.5f * 1.1f, ForceMode.Impulse);
                }
                else
                {
                    rb.AddForce(dashDir * dashForce * 0.5f, ForceMode.Impulse);
                }
            }

            dashFeedback.PlayFeedbacks();
            StartCoroutine(DashCooldown());
        }

        dashImage.fillAmount -= 1 / dashCooldown * Time.deltaTime;
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Jump()
    {
        if ((IsGrounded() || isWalljump) && !jumping)
        {
            canJump = true;
        }
        else
        {
            StartCoroutine(JumpLeway());
        }

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            jumping = true;
            canJump = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (isWalljump)
            {
                if(wallJumps < 5)
                {
                    var dir = ((Vector3.up * 3) + wallJumpDir).normalized;
                    rb.AddForce(dir * (jumpForce * 1.3f), ForceMode.Impulse);
                    wallJumps++;
                    jumpFeedback.PlayFeedbacks();
                }

            }
            else
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpFeedback.PlayFeedbacks();
            }

            StartCoroutine(JumpDelay());
        }
    }

    private IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(jumpDelay * 2.8f);
        jumping = false;
    }

    private IEnumerator JumpLeway()
    {
        yield return new WaitForSeconds(jumpLeway);
        canJump = false;
    }

    private bool IsGrounded() 
    {
        Collider[] colliders = Physics.OverlapBox(checkerPosition + transform.position, checkerCollider,transform.rotation, groundLayer);

        if(colliders.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 GetSurfaceNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayer))
        {
            return hit.normal;
        }

        return Vector3.up; // Default to returning upward direction if no surface is found
    }

    public void WallJumpChecker()
    {
        var colliders = Physics.OverlapBox(transform.position, new Vector3(1.2f, 0.9f, 1.2f), transform.rotation, wallJumpLayer);

        if (colliders.Length > 0)
        {
            if(!IsGrounded())
            {
                isWalljump = true;
            }

            foreach (var x in colliders)
            {
                var dir = transform.position - x.transform.position;
                dir.y = 0f;
                dir.Normalize();

                wallJumpDir = dir;
            }

        }
        else
        {
            isWalljump = false;
        }

        //counter walljump
        if(isWalljump && !IsGrounded())
        {
            wallDur += Time.deltaTime;

            float wallforce = 10f * wallDur;

            rb.AddForce(Vector3.down * wallforce);
        }
        else 
        {
            wallDur = 0f;
        }

        if (IsGrounded())
        {
            wallJumps = 0;
        }

        //Debug.Log(wallJumps + " and" + isWalljump + " is grounded: " + IsGrounded() + "is jumping: " + jumping + " canjump: " + canJump);
    }

    private void ParticleHandler()
    {
        if (IsGrounded())
        {
            walkingParticle.Play();
        }
        else
        {
            walkingParticle.Pause();
        }

        if (IsGrounded())
        {
            StartCoroutine(landedDelay());
        }

        if(rb.linearVelocity.y < -1f && !landed && IsGrounded())
        {
            landFeedback.PlayFeedbacks();
            landed = true;
        }
    }

    private IEnumerator landedDelay ()
    {
        yield return new WaitForSeconds(0.2f);
        landed = false;
    }

    private void Swing()
    {
        lastClickedTime = Time.time;
        noOfClicks++;
        if(noOfClicks == 1)
        {
            swordAnim.SetTrigger("hit1");
        }
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);

        if(noOfClicks == 2) 
        {
            swordAnim.SetTrigger("hit2");
        }

        if (noOfClicks == 3)
        {
            swordAnim.SetTrigger("hit3");
        }
    }

    public void GetSword()
    {
        SwordObject.SetActive(true);
        hasSword = true;
        scaleFeedback.AnimateScaleTarget = SwordObject.transform;
        powerupFeedback.PlayFeedbacks();
        canClick = true;
    }

    private void SwordHandler()
    {
        if (hasSword)
        {
            if (swordAnim.GetCurrentAnimatorStateInfo(0).IsName("Reset 1"))
            {
                noOfClicks = 0;
                canClick = true;
                isSwinging = false;
            }

            if (swordAnim.GetCurrentAnimatorStateInfo(0).IsName("Reset 2"))
            {
                noOfClicks = 0;
                canClick = true;
                isSwinging = false;
            }

            if(swordAnim.GetCurrentAnimatorStateInfo(0).IsName("Transition 1"))
            {
                isSwinging = false;
            }

            if (swordAnim.GetCurrentAnimatorStateInfo(0).IsName("Transition 2"))
            {
                isSwinging = false;
            }

            if (Time.time - lastClickedTime > maxComboDelay)
            {
                noOfClicks = 0;
            }
            if (canClick)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    canClick = false;
                    Swing();
                }
            }

            if (swordAnim.GetCurrentAnimatorStateInfo(0).IsName("SwordIdle"))
            {
                isSwinging = false;
                swordAnim.SetBool("hit2", false);
                swordAnim.SetBool("hit3", false);
                if (noOfClicks >= 2)
                {
                    noOfClicks = 0;
                    canClick = true;
                }

            }
            /*
            else
            {
                isSwinging = true;
            }
            */
        }

    }

    public void SwingDash()
    {
        Vector3 dashDir = Quaternion.Euler(0, cameraHolder.rotation.eulerAngles.y, 0) * Vector3.forward;
        dashDir.Normalize();

        swingDir = dashDir;

        
        Quaternion toRotation = Quaternion.LookRotation(dashDir, GetSurfaceNormal());
        transform.rotation = toRotation;

        swordRotTimer = 0f;

        if (IsGrounded())
        {
            rb.AddForce(dashDir * swingDashForce, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(dashDir * swingDashForce * 0.2f, ForceMode.Impulse);
        }
    }

    public void SwingStart()
    {
        isSwinging = true;
    }
    public void SwingEnd()
    {
        isSwinging = false;
    }

    public void GetShield()
    {
        hasShield = true;
        ShieldObject.SetActive(true);
        scaleFeedback.AnimateScaleTarget = ShieldObject.transform;
        powerupFeedback.PlayFeedbacks();
    }

    private void ShieldHandler()
    {
        if (Input.GetMouseButtonDown(1) && preGrapple == null && shieldReady && hasShield)
        {
            shieldReady = false;
            StartCoroutine(ShieldCooldown());
            button.material = buttonColorRed;
            shieldAnim.SetTrigger("block");
        }
    }

    private IEnumerator ShieldCooldown()
    {
        yield return new WaitForSeconds(shieldCooldown);
        button.material = buttonColorGreen;
        shieldReady = true;
    }

    public void GetGrapple()
    {
        hasGrapple = true;
        grappleObject.SetActive(true);
        scaleFeedback.AnimateScaleTarget = grappleObject.transform;
        powerupFeedback.PlayFeedbacks();
    }

    private void GrappleHandler()
    {
        if (hasGrapple)
        {
            Collider[] grapple = Physics.OverlapCapsule((mainCamera.position), mainCamera.position + (mainCamera.forward * grappleLenght), grappleRadius, grappleLayer);

            if (grapple.Length > 0 && !isSwinging && !isGrappeling)
            {
                foreach (Collider point in grapple)
                {
                    float dist = Vector3.Distance(mainCamera.position, point.transform.position);

                    if (preGrapple == null)
                    {
                        preGrappleDistance = dist;
                        preGrapple = point.transform;
                    }
                    else if (grappleDistance > dist)
                    {
                        preGrapple.GetComponent<Outline>().enabled = false;
                        preGrappleDistance = dist;
                        preGrapple = point.transform;
                    }
                }
                preGrapple.GetComponent<Outline>().enabled = true;
            }
            else
            {
                if (preGrapple != null && !isGrappeling)
                {
                    preGrapple.GetComponent<Outline>().enabled = false;
                    preGrapple = null;
                }
            }

            if (!isSwinging)
            {

                if (Input.GetMouseButtonUp(1))
                {
                    if (grappleTransform != null)
                    {
                        grappleTransform.GetComponent<Outline>().enabled = false;
                        grappleTransform = null;
                        Destroy(joint);
                        isGrappeling = false;
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Collider[] collider = Physics.OverlapCapsule((mainCamera.position), mainCamera.position + (mainCamera.forward * grappleLenght), grappleRadius, grappleLayer);

                    if (collider.Length > 0)
                    {
                        foreach (Collider point in collider)
                        {
                            float dist = Vector3.Distance(mainCamera.position, point.transform.position);

                            if (grappleTransform == null)
                            {
                                grappleDistance = dist;
                                grappleTransform = point.transform;
                            }
                            else if (grappleDistance > dist)
                            {
                                grappleDistance = dist;
                                grappleTransform = point.transform;
                            }
                        }

                        grappleTransform.GetComponent<Outline>().enabled = true;
                        joint = transform.gameObject.AddComponent<SpringJoint>();
                        joint.autoConfigureConnectedAnchor = false;

                        joint.connectedAnchor = grappleTransform.position;

                        joint.maxDistance = grappleDistance * 0.3f;
                        joint.minDistance = 1f;

                        joint.spring = 4.5f;
                        joint.damper = 7f;
                        joint.massScale = 4.5f;

                        isGrappeling = true;
                        currentGrapplePosition = transform.position;
                        grappleFeedback.PlayFeedbacks();

                    }
                    else
                    {
                        grappleTransform = null;
                    }
                }
            }

            if (isGrappeling)
            {
                Vector3 grappleUp = grappleTransform.position - grappleGun.position;
                grappleUp.Normalize();

                Quaternion toRotation = Quaternion.FromToRotation(grappleGun.forward, grappleUp) * grappleGun.rotation;

                grappleGun.rotation = Quaternion.RotateTowards(grappleGun.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                Quaternion toRotation = Quaternion.LookRotation(transform.forward, GetSurfaceNormal());

                grappleGun.rotation = Quaternion.RotateTowards(grappleGun.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void SoundHandler()
    {

            var minVolume = 0f;
            var maxVolume = 0.1f;
            var minSpeed = -10f;
            var maxSpeed = 30f;

        var airSound = transform.AddComponent<AudioSource>();

            float currentSpeed = rb.linearVelocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01((currentSpeed - minSpeed) / (maxSpeed - minSpeed));
            float volume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);

            float pitch = Mathf.Lerp(2, 2.5f, normalizedSpeed);

            float deltaVolume = (volume - airSound.volume) * 0.1f * Time.deltaTime;
            airSound.volume += deltaVolume;
            float deltaPitch = (pitch - airSound.pitch) * 0.1f * Time.deltaTime;
            airSound.pitch += deltaPitch;


    }

    public void TakeDamage()
    {
        if(elapsedTime > 1f * 2.8f && !isBlocking)
        {
            elapsedTime = 0f;
            takeDamageFeedback.PlayFeedbacks();
            gameController.TakeDamage(1);
        }
    }

    public void OutOfBoundsDamage()
    {
        if(elapsedTime > 1f * 2.8f)
        {
            takeDamageFeedback.PlayFeedbacks();
            elapsedTime = 0f;
            gameController.TakeDamage(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.tag == "Enemy")
        {

            var enemy = other.GetComponentInParent<Enemy>();
            if(enemy != null )
            {
                if (enemy.isHitting)
                {
                    TakeDamage();
                }
            }
            else
            {
                var enemy2 = other.GetComponent<Enemy>();
                if(enemy2 != null)
                {
                    if (enemy2.isHitting)
                    {
                        Debug.Log("PLAYER HIT BY 2");

                        TakeDamage();
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (rb != null && isGizmo)
        {
            Vector3 rel = rb.linearVelocity;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + dir * dirVel * gizmoStreanght);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) * gizmoStreanght);
   
        }

        if(isGizmo2)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + swingDir * 5f);
        }

    }
}
