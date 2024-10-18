using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Rigidbody _rb;
    public Transform _target;
    [SerializeField]
    private Transform playerTrans;

    [Header("MOVEMENT")]
    [SerializeField] private float _speed = 15;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float _rotateSpeed = 95;

    [Header("PREDICTION")]
    [SerializeField] private float _maxDistancePredict = 100;
    [SerializeField] private float _minDistancePredict = 5;
    [SerializeField] private float _maxTimePrediction = 5;
    private Vector3 _standardPrediction, _deviatedPrediction;

    [Header("DEVIATION")]
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    [Header("Explosion")]
    [SerializeField] private float exploRadius;
    [SerializeField] private float exploForce;
    [SerializeField] private LayerMask exploMask;
    [SerializeField] private GameObject _explosionPrefab;

    public Material blockedMaterial;
    public Material blockedTrailMaterial;


    private float duration = 10f * 2.8f;
    public float elapsedTime = 0f;

    private float duration2 = 0.5f * 2.8f;
    public Rigidbody tarRB;
    public Transform enemy;

    private List<Rigidbody> gotHit = new List<Rigidbody>();

    public void SetTarget(Transform target)
    {
        _target = target;
        if(playerTrans == null)
        {
            playerTrans = _target;
        }

        if (_target == null)
        {
            Detonate();
        }
        else
        {
            tarRB = _target.GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float t = elapsedTime / duration;
        var newSpeed = Mathf.Lerp(_speed, maxSpeed, t);
        _rb.linearVelocity = transform.forward * newSpeed;

        if (_target != null)
        {
            var leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));

            PredictMovement(leadTimePercentage);

            AddDeviation(leadTimePercentage);

            RotateRocket();
        }
    }

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);

        _standardPrediction = tarRB.position + tarRB.linearVelocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * _deviationSpeed), 0, 0);

        var predictionOffset = transform.TransformDirection(deviation) * _deviationAmount * leadTimePercentage;

        _deviatedPrediction = _standardPrediction + predictionOffset;
    }

    private void RotateRocket()
    {
        var heading = _deviatedPrediction - transform.position;

        float t = elapsedTime / duration2;
        var newRotSpeed = Mathf.Lerp(0, _rotateSpeed, t);

        var rotation = Quaternion.LookRotation(heading);
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, newRotSpeed * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Detonate();
    }

    private void Detonate()
    {
        Collider[] collidersHit = Physics.OverlapSphere(transform.position, exploRadius, exploMask);
        if (collidersHit.Length > 0)
        {
            foreach (Collider x in collidersHit)
            {
                var rigid = x.GetComponent<Rigidbody>();
                if (rigid != null)
                {
                    if (!gotHit.Contains(rigid))
                    {
                        gotHit.Add(rigid);

                        var player = rigid.GetComponent<CharacterController>();
                        if (player != null)
                        {
                            if (player.isBlocking)
                            {
                                continue;
                            }
                            else if(_target == playerTrans)
                            {
                                player.TakeDamage();
                            }
                        }
                        //rigid.AddExplosionForce(exploForce, transform.position, exploRadius);
                        var enemy2 = rigid.GetComponent<EnemyRanged>();
                        var enemy1 = rigid.GetComponent<Enemy>();

                        if (enemy2 != null)
                        {
                            enemy2.Hit();
                        }
                        if (enemy1 != null)
                        {
                            enemy1.Hit();
                        }

                        if(_target != player)
                        {
                            continue;
                        }

                        Vector3 dir = (rigid.transform.position - transform.position).normalized;
                        rigid.AddForce(dir * (exploForce), ForceMode.Impulse);
                    }

                }
            }
        }

        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _standardPrediction);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_standardPrediction, _deviatedPrediction);
    }

}
