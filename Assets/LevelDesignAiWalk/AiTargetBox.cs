using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTargetBox : MonoBehaviour
{
    private Enemy[] enemies;
    private EnemyRanged[] enemiesRanged;

    private void Awake()
    {
        enemies = GetComponentsInChildren<Enemy>();
        enemiesRanged = GetComponentsInChildren<EnemyRanged>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach(Enemy ai in enemies)
            {
                ai.CanAttack();
            }

            foreach(EnemyRanged ai2 in enemiesRanged)
            {
                ai2.CanAttack();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (Enemy ai in enemies)
            {
                ai.CannotAttack();
            }

            foreach (EnemyRanged ai2 in enemiesRanged)
            {
                ai2.CannotAttack();
            }
        }
    }


}
