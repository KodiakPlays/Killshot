using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform playerShip;
    [SerializeField] float speed = 10f;
    [SerializeField] float rotationSpeed = 0.2f;
    public bool isEnemyDetect;
    public bool isShootStart;
    [SerializeField] EnemyAttack enemyAtackScript;
    [SerializeField] float detectionRadius;
    [SerializeField] float shootRadius;
    [SerializeField] LayerMask playerLayer;

    void Update()
    {
        SpaceshipDetection();
        StartShooting();
       
        if (isEnemyDetect)
        {
            //Turn();
            //Move();

        }
        
        if(isShootStart)
        {
            enemyAtackScript.enabled = true;
            Turn();
            Move();
        }
        else
        {
            enemyAtackScript.enabled = false;
        }
    }
    ///<summary>
    ///This use to set Turn of Enemyship towards the playership
    ///</summary>
    void Turn()
    {
        Vector3 pos = playerShip.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
    ///<summary>
    ///This use to Move Enemy ship forward 
    ///</summary>
    void Move()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    ///<summary>
    ///This function use to detect the player ship using Physics.OverlapSphere() function
    ///</summary>
    void SpaceshipDetection()
    {
        isEnemyDetect = false;
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        foreach (Collider collider in hitCollider)
        {
            if(collider.gameObject.name == "Spaceship")
            {
                isEnemyDetect = true;
                break;
            }
            
        }
    }
    void StartShooting()
    {
        isShootStart = false;
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, shootRadius, playerLayer);
        foreach (Collider collider in hitCollider)
        {
            if (collider.gameObject.name == "Spaceship")
            {
                isShootStart = true;
                break;
            }

        }
    } 
}
