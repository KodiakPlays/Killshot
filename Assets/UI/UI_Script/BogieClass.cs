using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BogieClass
{
    public GameObject go;
    public Mesh mesh;
    public GameObject wepImageGo;
    public Material matWep;

    public BogieClass(GameObject go, Mesh mesh, GameObject wepImageGo, Material matWep)//need more data like stats etc
    {
        this.go = go;
        this.mesh = mesh;
        this.wepImageGo = wepImageGo;
        this.matWep = matWep;
    }

    public void WeapStart()
    {
        matWep.SetInt("_LaserFire", 0);
        matWep.SetInt("_RangeVisOn", 0);
        matWep.SetInt("_StabilityBool", 0);
        matWep.SetVector("_RangeColor", new Vector3(1f, 0f,0f));
    }


}
