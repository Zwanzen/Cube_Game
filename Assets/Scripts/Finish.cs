using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{

    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject box;

    bool won;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == player && !won)
        {
            won = true;
            gameController.Victory();
            box.SetActive(true);
        }
    }

}
