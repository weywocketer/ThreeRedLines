using UnityEngine;

public class RegimentIcon : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    RegimentController regimentController;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        regimentController = GetComponentInParent<RegimentController>();
    }

}
