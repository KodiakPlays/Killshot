using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    [SerializeField] float laserOffTime = 0.5f;
    [SerializeField] float maxDistance = 300f;
    public float fireDelay = 2f;
    LineRenderer lineRenderer;
    bool canFire;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void Start()
    {
        lineRenderer.enabled = false;
        canFire = true;
    }
    private void Update()
    {
        //FireLaser(transform.forward * maxDistance);
    }
    public void FireLaser(Vector3 targetPos, Transform target = null) // fore enemy to shoot player
    {
        if (canFire)
        {
            //if(target != null)
            //{
            //    SpawnExplosion(targetPos, target);
            //}
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, targetPos);
            lineRenderer.enabled = true;
            canFire = false;
            Invoke("TurnOffLaser", laserOffTime);
            Invoke("CanFire", fireDelay);
        }

    }
    public void FireLaser() // fore player to shoot enemy
    {
        Vector3 pos = CastRay();// FireLaser(CastRay());
        FireLaser(pos);
    }
    void TurnOffLaser()
    {
        lineRenderer.enabled = false;
    }
    void CanFire()
    {
        canFire = true;
    }
    public float Distance
    {
        get { return maxDistance; }
    }

    void ActiveWinPanle()
    {
        GameManager.Instance.GameWinPanale.SetActive(true);
    }
    Vector3 CastRay()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward) * maxDistance;
        if (Physics.Raycast(transform.position, fwd, out hit))
        {
            Debug.Log("hit to " + hit.transform.name);
            if (hit.transform.name == "Enemy" || hit.transform.gameObject.tag == "Rock")
            {
                Destroy(hit.transform.gameObject);
                Invoke("ActiveWinPanle", 0.5f);
                AudioManager.Instance.PlayEnemyExplosion();

            }

            return hit.point;
        }
        return transform.position + (transform.forward * maxDistance);
    }
    void SpawnExplosion(Vector3 hitPos, Transform target)
    {
        Explosion temp = target.GetComponent<Explosion>();
        if (temp != null)
        {
            temp.IveBeenHit(hitPos);
        }
    }
}
