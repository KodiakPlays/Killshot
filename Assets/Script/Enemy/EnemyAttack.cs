using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] GameObject Laser;
    [SerializeField] GameObject GameOverPanale;
    [SerializeField] GameObject hitGO;
    [SerializeField] float shipDamage;
     Vector3 hitPosition;
    [SerializeField] Laser laser;
    Damageable damageable;
    bool isDamage;
    private void Start()
    {
        isDamage = false;
    }
    void Update()
    {
        if(Infrount() && HaveLineOfSight())
        {
            FireLaser();
        }
    }
    ///<summary>
    ///It checks if the target is in front of the enemy or not
    ///</summary>
    bool Infrount()
    {
        Vector3 directionToTarget = transform.position - target.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if(Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 270)
        {
            //Debug.DrawLine(transform.position, target.position, Color.green);
            return true;
        }
        //Debug.DrawLine(transform.position, target.position, Color.yellow);
        return false;
    }
    ///<summary>
    ///It checks if the target HaveLineOfSight of the enemy or not
    ///</summary>
    bool HaveLineOfSight()
    {
        RaycastHit hit;
        Vector3 direction = target.position - transform.position;
        if(Physics.Raycast(laser.transform.position, direction, out hit, laser.Distance))
        {
            if (hit.transform.CompareTag("Spaceship"))
            {
                hitPosition = hit.transform.position;
               // Debug.Log(hitPosition + " hitPosition");
               // Debug.Log(hit.transform.name + " hitPosition");
                damageable = hit.transform.gameObject.GetComponent<Damageable>();
                hitGO = hit.transform.gameObject;
                return true;
            }
        }
        return false;
    }
    
    void FireLaser()
    {
        laser.FireLaser(hitPosition, target);
        if (!isDamage)
        {
            damageable.ApplyDamage(shipDamage);
            isDamage = true;
            Invoke("CanDamage", laser.fireDelay);
            Debug.Log("damageable.currentHealth: " + damageable.currentHealth);
            if (damageable.currentHealth <= 0)
            {
                Debug.Log("destroy ship");
                Destroy(hitGO);
                Invoke("ActiveGOPanal", 1);

            }
            
        }
        
    }
    void ActiveGOPanal()
    {
        GameOverPanale.SetActive(true);
    }
    void CanDamage()
    {
        isDamage = false;
    }
}
