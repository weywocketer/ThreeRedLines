using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapBoundry : MonoBehaviour
{
    private RtsController rtsController;
    [SerializeField] private bool right = true;

    // Start is called before the first frame update
    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
        if (right)
        {
            transform.position = new Vector3((rtsController.mapSizeX / 2) + 1, transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(-(rtsController.mapSizeX / 2) - 1, transform.position.y, transform.position.z);
        }
    }

}
