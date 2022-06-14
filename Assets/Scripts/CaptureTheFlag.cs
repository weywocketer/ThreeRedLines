using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureTheFlag : MonoBehaviour
{
    //[SerializeField] FlagController flagController;
    RtsController rtsController;

    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
    }

    void Update()
    {
        //rtsController.blueRegiments
    }
}
