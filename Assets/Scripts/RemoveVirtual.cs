using UnityEngine;

public class RemoveVirtual : MonoBehaviour
{
    private RtsController rtsController;
    RegimentController regimentController;

    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
        regimentController = GetComponent<RegimentController>();
        rtsController.startBattleEvent.AddListener(DestroyVirtualCompany);
    }

    void DestroyVirtualCompany()
    {
        regimentController.DestroyRegiment();
    }
}
