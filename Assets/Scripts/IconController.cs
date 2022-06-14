using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class IconController : MonoBehaviour
{
    [SerializeField] private Image panelUI;
    [SerializeField] public Image typeSymbolUI;
    [SerializeField] public TextMeshProUGUI regimentNameUI;
    [SerializeField] public TextMeshProUGUI soldierCountUI;
    [SerializeField] public TextMeshProUGUI moraleUI;
    [SerializeField] public TextMeshProUGUI staminaUI;
    [SerializeField] private Button buttonUI;
    [SerializeField] public TextMeshProUGUI disorganisedUI;
   

    private RtsController rtsController;
    public GameObject regiment;

    // Start is called before the first frame update
    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
        //regimentController = regiment.GetComponent<RegimentController>();
        buttonUI.onClick.AddListener(ManageSelection);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ManageSelection()
    {
        rtsController.ManageSelection(regiment);
    }

    private void OnMouseDown()
    {
        Debug.Log("collider");
    }

    public void SetColor()
    {
        if (rtsController.selectedRegiments.Contains(regiment))
        {
            panelUI.color = new Color(0, 0, 0, 0.5f);
        }
        else
        {
            panelUI.color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void UpdateCompanyIcon()
    {
        soldierCountUI.text = regiment.GetComponent<RegimentController>().soldierCount.ToString();
        moraleUI.text = "Morale: " + regiment.GetComponent<RegimentController>().morale.ToString();
        staminaUI.text = "Stamina: " + regiment.GetComponent<RegimentController>().stamina.ToString();
        disorganisedUI.gameObject.SetActive(regiment.GetComponent<RegimentController>().disorganised);
    }
}
