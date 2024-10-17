using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField]
    CharacterController player;
    [SerializeField]
    private playerItem Item;
    [SerializeField]
    private GameObject displayItem;
    [SerializeField]
    private ParticleSystem[] particles;
    [SerializeField]
    private float rotationSpeed = 1.0f;

    private bool taken = false;

    public enum playerItem
    {
        Sword,
        Shield,
        Grapple
    }

    private void ParticleOff()
    {
        foreach (var part in particles)
        {
            part.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!taken)
        {
            displayItem.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !taken)
        {
            if(Item == playerItem.Sword)
            {
                player.GetSword();
                displayItem.SetActive(false);
                ParticleOff();
                taken = true;
            }else if(Item == playerItem.Shield)
            {
                player.GetShield();
                displayItem.SetActive(false);
                ParticleOff();
                taken = true;
            }else if(Item == playerItem.Grapple)
            {
                player.GetGrapple();
                displayItem.SetActive(false);
                ParticleOff();
                taken = true;
            }
        }
    }

}
