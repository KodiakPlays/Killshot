using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        beam = this.gameObject.GetComponent<LineRenderer>();
        beam.material = laserMat;
        beam.useWorldSpace = true;
        beam.enabled = false;
    }

    void Activate()
    {
        beam.enabled = true;
        Debug.Log("Activate");
    }
    void DeActivate()
    {
        beam.enabled = false;

        beam.SetPosition(0, startPoint.position);
        beam.SetPosition(1, startPoint.position);
        Debug.Log("DeActivate");

    }
    // Update is called once per frame
    void Update()
    {
        //Activate();
        if (Input.GetMouseButtonDown(0))
        {
            beam.enabled = true;
            Activate();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            beam.enabled = false;
            DeActivate();
        }
        //if (Input.GetButton("Fire1"))
        //{
        //    beam.enabled = true;
        //    //beam.SetPosition(0, startPoint.position);
        //    //beam.SetPosition(1, startPoint.forward);
        //}
        //if (Input.GetButtonUp("Fire1"))
        //{
        //    beam.enabled = false;
        //}

    }
    private void FixedUpdate()
    {
        //if (!beam.enabled)
        //{
        //    return;
        //}
        Ray ray = new Ray(startPoint.position, startPoint.forward);
        RaycastHit hit;
        bool cast = Physics.Raycast(ray, out hit, maxDistance);
        Vector3 hitPosition = cast ? hit.point : startPoint.position + new Vector3(startPoint.position.x * maxDistance, startPoint.position.y , startPoint.position.z );//startPoint.up * maxDistance;

        beam.SetPosition(0, startPoint.position + new Vector3(0, 1.1f,0));
        beam.SetPosition(1, hitPosition);
        //if (cast & hit.collider.TryGetComponent(out Damageable damageable))
        //{
        //    damageable.ApplyDamage(damage);
        //}
    }
    
}
