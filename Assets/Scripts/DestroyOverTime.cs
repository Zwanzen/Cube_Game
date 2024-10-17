using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{

    [SerializeField]
    private float DestroyDelay = 5f;

    private void Awake()
    {
        Destroy(gameObject,DestroyDelay * 2.8f);
    }

}
