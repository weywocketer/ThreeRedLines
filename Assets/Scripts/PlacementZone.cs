using UnityEngine;

public class PlacementZone : MonoBehaviour
{
    public RtsController rtsController;

    // Start is called before the first frame update
    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
        rtsController.startBattleEvent.AddListener(DestroyPlacementZone);
    }

    void DestroyPlacementZone()
    {
        Destroy(gameObject);
    }
}
