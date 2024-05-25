using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform playerSubmarine;
    [SerializeField] float speed = 0.01f;
    public bool isEnemyDetect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isEnemyDetect)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerSubmarine.position, speed);
        }
        
    }
}
