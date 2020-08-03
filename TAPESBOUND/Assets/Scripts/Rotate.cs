using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotSpeed = 1f;

    void Update()
    {
        transform.Rotate(Vector3.forward * rotSpeed);
    }
}
