using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    [SerializeField] float laserOffTime = 0.5f;
    [SerializeField] float maxDistance = 300f;
    public float fireDelay = 2f;
    LineRenderer lineRenderer;
    [SerializeField] bool canFire;
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
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, targetPos);
            canFire = false;
            Invoke("TurnOffLaser", laserOffTime);
            Invoke("CanFire", fireDelay);
        }

    }
    public void FireLaser() // fore player to shoot enemy
    {
        Debug.Log("FireLaser");
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

    void ActiveWinPanel()
    {
        GameManager.Instance.GameWinPanale.SetActive(true);
        Time.timeScale = 0;
    }
    void ActiveEscapePoint()
    {
        GameManager.Instance.EscapePoint.SetActive(true);
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
                Debug.Log("hit.transform.gameObject: " + hit.transform.gameObject);
                //Invoke("ActiveWinPanel", 0.5f);
                //AudioManager.Instance.PlayEnemyExplosion();
                ActiveEscapePoint();
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
