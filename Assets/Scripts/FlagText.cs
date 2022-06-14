using UnityEngine;
using UnityEngine.UI;

public class FlagText : MonoBehaviour
{
    Text text;
    RegimentController regimentController;

    void Start()
    {
        text = GetComponent<Text>();
        regimentController = GetComponentInParent<RegimentController>();

        text.text = regimentController.regimentName;
    }
}
