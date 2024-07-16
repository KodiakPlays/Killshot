using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform playerSubmarine;
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
        SpaseshipDetection();
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
    void Turn()
    {
        Vector3 pos = playerSubmarine.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
    void Move()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    void SpaseshipDetection()
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
