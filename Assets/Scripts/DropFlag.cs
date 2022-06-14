using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFlag : MonoBehaviour
{
    public bool drop = false;

    void Update()
    {
        Debug.Log(transform.localRotation.eulerAngles.z);
        if (drop)
        {
            transform.Rotate(Vector3.forward, -100 * Time.deltaTime);
        }

        if (transform.localRotation.eulerAngles.z < 270)
        {
            drop = false;
        }
    }

}
