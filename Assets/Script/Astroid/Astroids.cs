using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astroids : MonoBehaviour
{
    [SerializeField] float minScale, maxScale;
    [SerializeField] float rotationOffset;
    Vector3 randomRotation;
    Transform trans;
    private void Awake()
    {
        trans = transform;
    }
    void Start()
    {
        RandomScaleAndRotation();
    }

    void Update()
    {
        RandomRotation();
    }
    void RandomRotation()
    {
        trans.Rotate(randomRotation * Time.deltaTime);
    }
    void RandomScaleAndRotation()
    {
        Vector3 scale = Vector3.one;
        scale.x = Random.Range(minScale, maxScale);
        scale.y = Random.Range(minScale, maxScale);
        scale.z = Random.Range(minScale, maxScale);

        trans.localScale = scale;

        //Random Rotation

        randomRotation.x = Random.Range(-rotationOffset, rotationOffset);
        randomRotation.y = Random.Range(-rotationOffset, rotationOffset);
        randomRotation.z = Random.Range(-rotationOffset, rotationOffset);
    }
}
