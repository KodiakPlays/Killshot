using UnityEngine;
using UnityEngine.UI;

public class BogieClass : MonoBehaviour
{
    public GameObject go;
    public Mesh mesh;
    public Image wepImage;
    public Material matWep;

    public BogieClass(GameObject go, Mesh mesh, Image wepImage, Material matWep)//need more data like stats etc
    {
        this.go = go;
        this.mesh = mesh;
        this.wepImage = wepImage;
        this.matWep = matWep;
    }

    public void WeapStart()
    {
        matWep.SetInt("_LaserFire", 0);
        matWep.SetInt("_RangeVisOn", 0);
        matWep.SetVector("_RangeColor", new Vector3(1f, 0f,0f));
    }


}
