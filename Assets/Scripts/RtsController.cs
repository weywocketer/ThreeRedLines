using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class RtsController : MonoBehaviour
{
    // "global" variables
    public Side playerSide = Side.Red;
    public float mapSizeX = 200.0f;
    float mapBoundryX;
    public float walkingPrecision = 0.1f;



    public List<GameObject> selectedRegiments;
    public List<GameObject> redRegiments;
    public List<GameObject> blueRegiments;
    LayerMask groundMask;
    LayerMask unitMask;

    [SerializeField] private Button formLineButton;
    [SerializeField] private Button quickMarchButton;
    [SerializeField] private Button marchMarchButton;
    [SerializeField] private Button holdFireButton;

    [SerializeField] private Button startBattleButton;
    [SerializeField] Image hintPanel;
    [SerializeField] Image outcomePanel;

    public bool placementPhase = true;
    public UnityEvent startBattleEvent;

    public List<Collider> placementZoneCollidersList;
    private GameObject placementZonesGameObject;


    
    

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;

        //Application.targetFrameRate = 30;

        formLineButton.onClick.AddListener(FormLine);
        quickMarchButton.onClick.AddListener(QuickMarch);
        marchMarchButton.onClick.AddListener(MarchMarch);
        holdFireButton.onClick.AddListener(HoldFire);

        startBattleButton.onClick.AddListener(StartBattle);
        groundMask = LayerMask.GetMask("PlacementZone");
        unitMask = LayerMask.GetMask("Unit");

        mapBoundryX = mapSizeX / 2;

        placementZonesGameObject = GameObject.Find("PlacementZones");

        for (int i = 0; i < placementZonesGameObject.transform.childCount; i++)
        {
            placementZoneCollidersList.Add(placementZonesGameObject.transform.GetChild(i).GetComponent<Collider>());
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
            
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            hintPanel.gameObject.SetActive(!hintPanel.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, unitMask))
            {
                //Debug.DrawLine(ray.origin, hit.point, Color.red, 30);

                if (hit.collider.gameObject.GetComponent<RegimentController>().playable)
                {
                    ManageSelection(hit.collider.gameObject);
                }
            }
            else
            {
                if (!EventSystem.current.IsPointerOverGameObject()) // in practice: check if the click was not on the UI
                {
                    ClearSelection();
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            float changeLine = 0;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                changeLine = Camera.main.transform.rotation.y == 0 ? 40 : -40;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                changeLine = Camera.main.transform.rotation.y == 0 ? -40 : 40;
            }


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, groundMask))
            {
                //Debug.DrawLine(ray.origin, hit.point, Color.blue, 30);

                MoveCommand(selectedRegiments, hit.point, changeLine);

                //foreach (GameObject regiment in selectedRegiments)
                //{

                //    if (placementPhase && regiment.GetComponent<RegimentController>().placable)
                //    {
                //        regiment.GetComponent<RegimentController>().PlaceCompany(hit.point);
                //    }
                //    else
                //    {
                //        //var a = Input.mousePosition;
                //        ////a.z = 19;
                //        //a.z = 20;
                //        //Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(a);
                //        //Debug.Log(mouseWorldPosition);
                //        //Debug.Log(Input.mousePosition);
                //        //mouseWorldPosition.y


                //        regiment.GetComponent<RegimentController>().task.destination.x = hit.point.x;
                //        //regiment.GetComponent<RegimentController>().task.destination.x = mouseWorldPosition.x;
                //        //regiment.GetComponent<RegimentController>().task.destination.y = regiment.GetComponent<RegimentController>().task.destination.y + changeLine;
                //        regiment.GetComponent<RegimentController>().task.destination.y = regiment.GetComponent<RegimentController>().task.destination.y + changeLine;

                //        regiment.GetComponent<RegimentController>().task.taskType = TaskType.Move;

                //        regiment.GetComponent<RegimentController>().StopReload();

                //        // ^ consider making method in RegimentController with all these things...

                //        //regiment.GetComponent<RegimentController>().StopCoroutine("Reload");
                //        //regiment.GetComponent<RegimentController>().reloading = false;
                //    }


                //}


            }



        }

        if ( Input.GetKeyDown(KeyCode.A) && !placementPhase && ( Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ) )
        {
            ClearSelection();
            selectedRegiments.AddRange(redRegiments.FindAll(regiment => regiment.GetComponent<RegimentController>().playable));
            selectedRegiments.AddRange(blueRegiments.FindAll(regiment => regiment.GetComponent<RegimentController>().playable));

            foreach (GameObject regiment in selectedRegiments)
            {
                regiment.GetComponent<RegimentController>().regimentIcon.GetComponent<IconController>().SetColor();
            }

            //target = enemyRegiments.Find(b => Vector3.Distance(transform.position, b.transform.position) == enemyRegiments.Min(a => Vector3.Distance(transform.position, a.transform.position)));
            //selectedRegiments.Add(regiment);
        }


        //if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        //{
        //    foreach (GameObject regiment in selectedRegiments)
        //    {
        //        GameObject.fin
        //        regiment.GetComponent<RegimentController>().iconRenderer.color = Color.blue;
        //    }
        //}


    }

    public void MoveCommand(List<GameObject> affectedRegiments, Vector3 location, float changeLine)
    {
        foreach (GameObject regiment in affectedRegiments)
        {

            if (placementPhase && regiment.GetComponent<RegimentController>().placable)
            {
                regiment.GetComponent<RegimentController>().PlaceCompany(location);
            }
            else
            {
                RegimentController regimentController = regiment.GetComponent<RegimentController>();

                if (!regimentController.fleeing)
                {
                    regimentController.task.destination.x = Mathf.Clamp(location.x, -mapBoundryX, mapBoundryX);
                    regimentController.task.destination.y = Mathf.Clamp(regiment.GetComponent<RegimentController>().task.destination.y + changeLine, -40, 40);

                    regimentController.task.taskType = TaskType.Move;

                    regimentController.StopReload();
                    regimentController.StopFireing();
                }

                // ^ consider making method in RegimentController with all these things...

                //regiment.GetComponent<RegimentController>().StopCoroutine("Reload");
                //regiment.GetComponent<RegimentController>().reloading = false;
            }


        }
    }

    public void ManageSelection(GameObject regiment) // operations done usaually when regiment (or its icon) was left clicked
    {
        if ( (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)) && !placementPhase)
        {
            if (selectedRegiments.Contains(regiment))
            {
                selectedRegiments.Remove(regiment);
            }
            else
            {
                selectedRegiments.Add(regiment);
            }
        }
        else
        {
            ClearSelection();
            selectedRegiments.Add(regiment);
        }

        regiment.GetComponent<RegimentController>().regimentIcon.GetComponent<IconController>().SetColor();
    }

    public void ClearSelection()
    {
        List<GameObject> tempSelectedR = new List<GameObject>(selectedRegiments); // create temp copy
        //rtsController.selectedRegiments.CopyTo(tempSelectedR);
        selectedRegiments.Clear();

        foreach (GameObject regiment in tempSelectedR)
        {
            regiment.GetComponent<RegimentController>().regimentIcon.GetComponent<IconController>().SetColor();
        }
    }

    bool CheckIfAllPlaced()
    {
        foreach (GameObject company in redRegiments)
        {
            if (company.GetComponent<RegimentController>().placable)
            {
                if (!CheckIfCompanyInZone(company))
                {
                    return false;
                }
            }
        }

        foreach (GameObject company in blueRegiments)
        {
            if (company.GetComponent<RegimentController>().placable)
            {
                if (!CheckIfCompanyInZone(company))
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool CheckIfCompanyInZone(GameObject company)
    {
        foreach (Collider zoneCollider in placementZoneCollidersList)
        {
            if (company.GetComponent<Collider>().bounds.Intersects(zoneCollider.bounds))
            {
                return true;
            }
        }

        return false;
    }

    public void removeCompany(GameObject company, Side side)
    {
        if (side == Side.Red)
        {
            redRegiments.Remove(company);
            if (redRegiments.Count == 0)
            {
                if (playerSide == Side.Red)
                {
                    FinishBattle(false);
                }
                else
                {
                    FinishBattle(true);
                }
            }
        }
        else if (side == Side.Blue)
        {
            blueRegiments.Remove(company);
            if (blueRegiments.Count == 0)
            {
                if (playerSide == Side.Blue)
                {
                    FinishBattle(false);
                }
                else
                {
                    FinishBattle(true);
                }
            }
        }
    }

    public void FormLine()
    {
        foreach (GameObject regiment in selectedRegiments)
        {
            regiment.GetComponent<RegimentController>().StartReformLine();
        }
    }

    public void QuickMarch()
    {
        foreach (GameObject regiment in selectedRegiments)
        {
            regiment.GetComponent<RegimentController>().QuickMarch();
        }
    }

    public void MarchMarch()
    {
        foreach (GameObject regiment in selectedRegiments)
        {
            regiment.GetComponent<RegimentController>().MarchMarch();
        }
    }

    public void HoldFire()
    {
        foreach (GameObject regiment in selectedRegiments)
        {
            regiment.GetComponent<RegimentController>().HoldFire();
        }
    }

    private void FinishBattle(bool playerWon)
    {
        

        if (playerWon)
        {
            outcomePanel.transform.GetComponentInChildren<Text>().text = "VICTORY!";
        }
        else
        {
            outcomePanel.transform.GetComponentInChildren<Text>().text = "DEFEAT";
        }

        outcomePanel.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void StartBattle()
    {
        if (CheckIfAllPlaced())
        {
            placementPhase = false;
            startBattleButton.gameObject.SetActive(false);
            groundMask = LayerMask.GetMask("Ground");
            startBattleButton.onClick.RemoveListener(StartBattle);
            startBattleEvent.Invoke();
        }
        else
        {
            // show information on the UI
        }

    }

}
