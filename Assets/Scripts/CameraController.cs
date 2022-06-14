using UnityEngine;

public class CameraController : MonoBehaviour
{
    float depthMovement = 40.0f;
    float depthMovementRate = 0;
    float cameraSpeed = 20;
    int reverse = 1;
    float mapBoundryX;

    //GameObject companyToTrack;

    RtsController rtsController;

    void Start()
    {
        rtsController = FindObjectOfType<RtsController>();
        mapBoundryX = rtsController.mapSizeX / 2;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (transform.parent)
            {
                //companyToTrack = null;
                transform.SetParent(null);
            }
            else
            {
                if (rtsController.selectedRegiments.Count != 0)
                {
                    //companyToTrack = rtsController.selectedRegiments[0];
                    //transform.position = new Vector3(companyToTrack.transform.position.x, transform.position.y, companyToTrack.transform.position.z - reverse * depthMovement / 2);
                    transform.SetParent(rtsController.selectedRegiments[0].transform);
                }
            }

        }
        
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                cameraSpeed *= 2;
                depthMovementRate *= 2;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                cameraSpeed /= 2;
                depthMovementRate /= 2;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (rtsController.selectedRegiments.Count != 0)
                {
                    transform.position = new Vector3(rtsController.selectedRegiments[0].transform.position.x, transform.position.y, rtsController.selectedRegiments[0].transform.position.z - reverse * depthMovement / 2);
                }
            }

            //if ( !( (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.A)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))  ) ) // if is used to resolve conflict between A key and ctrl+A shortcut
            //{
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * cameraSpeed);
            //}

            if (transform.position.x > mapBoundryX)
            {
                transform.position = new Vector3(mapBoundryX, transform.position.y, transform.position.z);
            }

            if (transform.position.x < -mapBoundryX)
            {
                transform.position = new Vector3(-mapBoundryX, transform.position.y, transform.position.z);
            }


            if (Mathf.Abs(Mathf.Abs(transform.position.z % depthMovement) - (depthMovement / 2)) < 1) // camera is in one of three lines
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z / 20.0f) * 20);

                if (Input.GetKeyDown(KeyCode.W) && (   ( reverse==1 && transform.position.z < 80 ) || ( reverse == -1 && transform.position.z > -80 )   ) )
                {
                    depthMovementRate = cameraSpeed * reverse;
                    transform.Translate(Vector3.forward * 0.06f * depthMovementRate, Space.World);
                    // fixed value (eg. 0.035f - should be appropriate to precision value in if statement) is used instead of delta, to prevent from "not leaving the line" when FPS is high enough 
                    // so: cameraSpeed * "fixed value" > precision
                }

                if (Input.GetKeyDown(KeyCode.S) && (   ( reverse==1 && transform.position.z > -80 ) || ( reverse == -1 && transform.position.z < 80 )   ) )
                {
                    depthMovementRate = -cameraSpeed * reverse;
                    transform.Translate(Vector3.forward * 0.06f * depthMovementRate, Space.World);
                }
            }
            else
            {
                transform.Translate(Vector3.forward * Time.deltaTime * depthMovementRate, Space.World);
            }


            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.Rotate(Vector3.up, 180.0f);
                reverse *= -1;
            }
        }
    }
}
