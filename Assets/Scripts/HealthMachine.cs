using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using MoreMountains.Feedbacks;

public class HealthMachine : MonoBehaviour
{

    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private Transform player;
    [SerializeField]
    private GameObject buyText;

    [SerializeField]
    private MMF_Player buyFeedback;
    [SerializeField]
    private MMF_Player failBuyFeedback;

    private float buyDist = 12f;

    private void Awake()
    {
        buyText.SetActive(false);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < buyDist && Input.GetKeyDown(KeyCode.F))
        {
            if(gameController.crystalAmount > 1 && gameController.maxHealth > gameController.currentHealth)
            {
                gameController.PurchaseHeart();
                buyFeedback.PlayFeedbacks();
            }
            else
            {
                failBuyFeedback.PlayFeedbacks();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            buyText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            buyText.SetActive(false);
        }
    }

}
