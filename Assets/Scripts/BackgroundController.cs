using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundController : MonoBehaviour
{
    RtsController rtsController;

    // Start is called before the first frame update
    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // in practice: check if the click was not on the UI
        {
            rtsController.ClearSelection();
        }
    }
}
