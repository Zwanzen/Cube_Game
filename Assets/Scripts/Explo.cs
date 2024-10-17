using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explo : MonoBehaviour
{
    public Transform Player;

    public GameObject explosionPrefab;
    public float explosionForce = 10f;
    public float explosionRadius = 5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 point = Player.position - Vector3.down;

                Vector3 explosionPosition = point;
                Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);

                Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
                foreach (Collider collider in colliders)
                {
                    Rigidbody rb = collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
                    }
                }
        }
    }
}
