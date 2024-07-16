using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroidManager : MonoBehaviour
{
    [SerializeField] GameObject astroid;
    [SerializeField] int numberOfAstroidOnAxis;
    [SerializeField] int gridSpace;
    // Start is called before the first frame update
    void Start()
    {
        PlaseAstroid();
    }
    void PlaseAstroid()
    {
        for (int x = -numberOfAstroidOnAxis; x < numberOfAstroidOnAxis; x++)
        {
            for(int y = -numberOfAstroidOnAxis; y < numberOfAstroidOnAxis; y++)
            {
                for(int z = -numberOfAstroidOnAxis; z< numberOfAstroidOnAxis; z++)
                {
                    InstantiateAstroid(x,y, z); 
                }
            }
        }
    }
    void InstantiateAstroid(int x, int y, int z)
    {
        Instantiate(astroid, new Vector3(
            transform.position.x + (x * gridSpace) + AstroidOffset(),
            transform.position.y + (y * gridSpace) + AstroidOffset(),
            transform.position.z + (z * gridSpace) + AstroidOffset()),
            Quaternion.identity,
            transform);
    }
    float AstroidOffset()
    {
        return Random.Range(-gridSpace / 2f, gridSpace / 2f);
    }
}
