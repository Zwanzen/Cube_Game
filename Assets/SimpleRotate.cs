using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    public float rotationSpeed = 10.0f;

    private void Update()
    {
        // slowly rotate the object around the Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

}
