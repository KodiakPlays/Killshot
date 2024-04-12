using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] LineRenderer beam;
    [SerializeField] float maxDistance = 100f;
    [SerializeField] Transform startPoint;
    public Material laserMat;

    [SerializeField] float damage;
   
    // Start is called before the first frame update
    void Start()
    {
        beam = GetComponent<LineRenderer>();
        beam.material = laserMat;
        beam.useWorldSpace = true;
        beam.enabled = false;
    }

    void Activate()
    {
        beam.enabled = true;
    }
    void DeActivate()
    {
        beam.enabled = false;

        beam.SetPosition(0, startPoint.position);
        beam.SetPosition(1, startPoint.position);
    }
    // Update is called once per frame
    void Update()
    {
        //Activate();
        if (Input.GetMouseButtonDown(0))
        {
            Activate();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            DeActivate();
        }
    }
    private void FixedUpdate()
    {
        if (!beam.enabled)
        {
            return;
        }
        Ray ray = new Ray(startPoint.position, startPoint.forward);
        RaycastHit hit;
        bool cast = Physics.Raycast(ray, out hit, maxDistance);
        Vector3 hitPosition = cast ? hit.point : startPoint.position + startPoint.forward * maxDistance;

        beam.SetPosition(0, startPoint.position);
        beam.SetPosition(1, hitPosition);
        if (cast & hit.collider.TryGetComponent(out Damageable damageable))
        {
            damageable.ApplyDamage(damage);
        }
    }
    
}
