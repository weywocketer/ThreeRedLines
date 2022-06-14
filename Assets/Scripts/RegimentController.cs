using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RegimentController : MonoBehaviour
{
    [Header("General")]
    public Side side = Side.Red;
    public CompanyType companyType = CompanyType.Infantry;
    public string regimentName;
    public string regimentFullName;
    public bool playable = true;
    public bool placable = true;
    
    public RegimentTask task = new RegimentTask();

    [Header("Spacing")]
    [SerializeField] public int startingSoldierCount = 100;
    [SerializeField] public int soldierCount;
    [SerializeField] float soldierSpacing = 0.6f;
    [SerializeField] float lineSpacing = 1.0f; // first line has offset=0
    public float xScatter = 0.15f; // definies range for soldier random offset on the x axis
    public float zScatter = 0.0f;


    [Header("Stats")]
    [SerializeField] float reloadRate = 3.0f;
    [SerializeField] int meleeSkill = 30; // percentile chance to kill enemy (per 1 soldier per 1 melee cycle)
    [SerializeField] float baseDamageReduction = 0;
    [SerializeField] float quickMarchSpeed = 3.0f;
    [SerializeField] float marchMarchSpeed = 5.0f;
    [SerializeField] float shootingRange = 70.0f;
    


    // march: 60 steps/minute
    // quick march: 120
    // march march: 140


    [Header("Prefab references")]
    [SerializeField] GameObject soldierObject;
    [SerializeField] GameObject regimentIconObject;
    //[SerializeField] public IconController iconController;


    [Header("I dunno")]
    [Range(0.1f, 30f)] public float speed;
    public int stamina = 100;  
    int maxMorale = 100;
    public int morale = 100;
    float speedModifier = 1; // 1 = no modifier
    float actualDamageReduction;


    [Header("States")]
    public bool disorganised = false;
    public bool afterReorganisation = false; // state that persists for a few seconds after reorganisation, to prevent unit from taking actions (move, reload, shoot, etc.) immediately after reorganisation
    public bool walking;
    public bool readyToFire = true;
    public bool reloading = false;
    public bool fireing = false;
    public bool meele = false;
    public bool holdFire = false;
    public bool fleeing = false;


    [Header("Layers")]
    [SerializeField] LayerMask unitMask;
    private readonly int unitLayer = 7;
    private readonly int terrainLayer = 8;
    //[SerializeField] LayerMask terrainMask;
    //public int lineNumber; // 1 -> left, 0 -> middle, -1 -> right


    [Header("Other GameObject references")]

    public GameObject shootingTarget;
    public GameObject meeleTargetFront;
    public GameObject meeleTargetBack;
    public GameObject regimentIcon;

    public List<List<GameObject>> soldierList = new List<List<GameObject>> { new List<GameObject>(), new List<GameObject>() };
    public List<GameObject> enemyRegiments;

    public RtsController rtsController;
    public SpriteRenderer iconRenderer;


    [Header("Events")]
    public UnityEvent animationChanged;
    public UnityEvent regimentIsBeingDestroyed;


    [Header("Constant values")]
    float mapBoundryX;
    private int facing = 1;
    public float walkingPrecision;
    private float musketInaccuracy;


   void OnEnable() // OnEnable instead of start, to allow FlagText.cs load initialized regimentName
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();
        iconRenderer = GetComponentInChildren<SpriteRenderer>();

        task.taskType = TaskType.HoldPosition;
        task.destination = new Vector2(transform.position.x, transform.position.z);


        if (regimentName == "")
        {
            regimentName = (Random.Range(1, 1000)).ToString();
            regimentFullName = $"{regimentName}th Infantry Company";
        }
        gameObject.name = regimentFullName;

        speed = quickMarchSpeed;
        soldierCount = startingSoldierCount;
        actualDamageReduction = baseDamageReduction;

        mapBoundryX = rtsController.mapSizeX / 2;
        walkingPrecision = rtsController.walkingPrecision;

        if (shootingRange == 70)
        {
            musketInaccuracy = 0.005f;
        }
        else if (shootingRange == 90)
        {
            musketInaccuracy = 0.003f;
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("shootingRange variable has improper value!");
        }

        //if (transform.position.z == 0)
        //{
        //    lineNumber = 0;
        //}
        //else if (transform.position.z == 40)
        //{
        //    lineNumber = 1;
        //}
        //else if (transform.position.z == -40)
        //{
        //    lineNumber = -1;
        //}



        //for (int i = 0; i < soldierCount; i++)
        //{
        //    Instantiate(soldierObject, transform.position + new Vector3(0, 0, 0.5f * i), transform.rotation, transform);
        //}
        //soldierList = new List<List<GameObject>>();

        //Debug.Log(soldierList[0]);
        //soldierList[0].Add(Instantiate(soldierObject, transform.position + new Vector3(0, 0, 0.5f), transform.rotation, transform));

        if (side == Side.Red)
        {
            rtsController.redRegiments.Add(gameObject);
            enemyRegiments = rtsController.blueRegiments;
        }
        else if (side == Side.Blue)
        {
            rtsController.blueRegiments.Add(gameObject);
            enemyRegiments = rtsController.redRegiments;
            transform.Rotate(Vector3.up, 180);
            facing = -1;
        }

        if (playable) // create UI icon for playable regiment
        {
            regimentIcon = Instantiate(regimentIconObject);
            regimentIcon.transform.SetParent(GameObject.Find("IconCanvas").transform, false);
            
            //regimentIcon.transform.position = new Vector3(119, -122, 0);
            //regimentIcon.transform.position = new Vector3(0, 0, 0);
            

            regimentIcon.GetComponent<IconController>().regiment = gameObject;
            regimentIcon.GetComponent<IconController>().regimentNameUI.text = regimentFullName;
            regimentIcon.GetComponent<IconController>().UpdateCompanyIcon();

            if (side == Side.Red)
            {
                regimentIcon.GetComponent<IconController>().typeSymbolUI.color = Color.red;
            }
            else if (side == Side.Blue)
            {
                regimentIcon.GetComponent<IconController>().typeSymbolUI.color = Color.blue;
            }
        }

        for (int i = 0; i < soldierCount/2; i++)
        {
            //soldierList[0].Add(Instantiate(soldierObject, transform.position + new Vector3(0, 0, 0.5f * i), transform.rotation, transform));
            soldierList[0].Add(Instantiate(soldierObject, transform.position, transform.rotation, transform));
        }

        for (int i = 0; i < soldierCount/2; i++)
        {
            //soldierList[0].Add(Instantiate(soldierObject, transform.position + new Vector3(0, 0, 0.5f * i), transform.rotation, transform));
            soldierList[1].Add(Instantiate(soldierObject, transform.position, transform.rotation, transform));
        }
        if(soldierCount%2 != 0) // if odd number, add odd to the first line
        {
            soldierList[0].Add(Instantiate(soldierObject, transform.position, transform.rotation, transform));
        }

        SetupLine();

        StartCoroutine("CalculateStamina");
        StartCoroutine(Melee());
    }

    void Update()
    {
        if(rtsController.placementPhase)
        {

        }
        else
        {
            if (fleeing)
            {
                walking = true;
                animationChanged.Invoke();

                // first two if's are for the case, when unit started fleeing while changing line
                if (task.destination.y - transform.position.z > walkingPrecision)
                {
                    transform.Translate(facing * Time.deltaTime * speed * speedModifier * Vector3.forward);
                }
                else if (task.destination.y - transform.position.z < -walkingPrecision)
                {
                    transform.Translate(facing * (-1) * Time.deltaTime * speed * speedModifier * Vector3.forward);
                }
                else if (side == Side.Red && !meeleTargetBack)
                {
                    transform.Translate(facing * (-1) * Time.deltaTime * speed * speedModifier * Vector3.right);
                    
                    if (transform.position.x < -mapBoundryX)
                    {
                        DestroyRegiment();
                    }
                }
                else if (side == Side.Blue && !meeleTargetBack)
                {
                    transform.Translate(facing * Time.deltaTime * speed * speedModifier * Vector3.right);

                    if (transform.position.x > mapBoundryX)
                    {
                        DestroyRegiment();
                    }
                }

            }
            else if (!afterReorganisation) // manage normal movement (tasks)
            {
                if (companyType != CompanyType.Cavalery && task.taskType == TaskType.HoldPosition && !disorganised && !meele) // HoldPosition actions (fireing, reloading...) are not performed when unit is disorganised
                {

                    if (!fireing) // fireing==true means that Fire coroutine is already excecuting, thus nothing needs to be done
                    {
                        if (readyToFire && !holdFire) // manage firing
                        {
                            //float c = enemyRegiments.Min(a => Vector3.Distance(transform.position, a.transform.position));
                            shootingTarget = enemyRegiments.Find(b => Vector3.Distance(transform.position, b.transform.position) == enemyRegiments.Min(a => Vector3.Distance(transform.position, a.transform.position)));
                            // ^ get the closest enemy regiment; consider some optimisation here...

                            if (shootingTarget != null)
                            {
                                if (Vector3.Distance(transform.position, shootingTarget.transform.position) <= shootingRange)
                                {

                                    // check for obstacles
                                    Ray ray = new Ray(transform.position, shootingTarget.transform.position - transform.position);
                                    RaycastHit hit;

                                    if (Physics.Raycast(ray, out hit, shootingRange, unitMask))
                                    {
                                        //Debug.DrawLine(ray.origin, hit.point, Color.magenta, 30);
                                        if (hit.collider.gameObject == shootingTarget)
                                        {
                                            StartCoroutine("Fire");
                                        }
                                    }

                                }
                            }
                        }
                        if (!reloading && !readyToFire)
                        {
                            StartCoroutine("Reload");
                        }
                    }

                }

                if (task.taskType == TaskType.Move) // manage movement && transform.position.x < mapBoundryX
                {
                    walking = true;
                    animationChanged.Invoke();
                    if (task.destination.y - transform.position.z > walkingPrecision)
                    {
                        transform.Translate(facing * Time.deltaTime * speed * speedModifier * Vector3.forward);
                    }
                    else if (task.destination.y - transform.position.z < -walkingPrecision)
                    {
                        transform.Translate(facing * (-1) * Time.deltaTime * speed * speedModifier * Vector3.forward);
                    }
                    else if (facing * (task.destination.x - transform.position.x) > walkingPrecision && !meeleTargetFront)
                    {
                        transform.Translate(Time.deltaTime * speed * speedModifier * Vector3.right);
                    }
                    else if (facing * (task.destination.x - transform.position.x) < -walkingPrecision && !meeleTargetBack)
                    {
                        transform.Translate((-1) * Time.deltaTime * speed * speedModifier * Vector3.right);
                    }
                    else
                    {
                        walking = false;
                        animationChanged.Invoke();

                        task.taskType = TaskType.HoldPosition;

                    }
                }
            }
        }
        

    }

    public void PlaceCompany(Vector3 position)
    {
        position.z = Mathf.Round(position.z / 40.0f) * 40;

        // temporary solution, y is set to zero to prevent problem with placement zone; must be solved when hills are implemented
        transform.position = new Vector3(position.x, 0, position.z);

        task.destination.x = position.x;
        task.destination.y = position.z;
    }

    public void SetupLine() // used only when company is created, to setup soldiers positions in formation
    {
        for (int i = 0; i < soldierList.Count; i++)
        {
            if (soldierList[i].Count%2 == 0)
            {
                for (int j = 0; j < soldierList[i].Count/2; j++)
                {
                    //soldierList[i][j].transform.Translate(Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, soldierSpacing/2));
                    soldierList[i][j].transform.localPosition = Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, soldierSpacing / 2);
                }
                for (int j = 0; j < soldierList[i].Count/2; j++)
                {
                    //soldierList[i][soldierList[i].Count/2 + j].transform.Translate(Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, -soldierSpacing/2));
                    soldierList[i][soldierList[i].Count / 2 + j].transform.localPosition = Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, -soldierSpacing / 2);
                }
            }
            else
            {
                for (int j = 0; j < soldierList[i].Count/2 + 1; j++)
                {
                    //soldierList[i][j].transform.Translate(Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, 0));
                    soldierList[i][j].transform.localPosition = Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, 0);
                }
                for (int j = 1; j < soldierList[i].Count/2 + 1; j++)
                {
                    //soldierList[i][soldierList[i].Count/2 + j].transform.Translate(Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, 0));
                    soldierList[i][soldierList[i].Count / 2 + j].transform.localPosition = Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, 0);
                }
            }

        }
        
    }

    public void StartReformLine()
    {
        StartCoroutine(ReformLine());
    }

    IEnumerator ReformLine()
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 4.0f));

        if (!fireing)
        {
            RefillFirstLine();
            for (int i = 0; i < soldierList.Count; i++)
            {
                if (soldierList[i].Count % 2 == 0)
                {
                    for (int j = 0; j < soldierList[i].Count / 2; j++)
                    {
                        //soldierList[i][j].transform.Translate(Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, soldierSpacing/2));
                        soldierList[i][j].GetComponent<SoldierController>().SetBasePositionAndOffsets(Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, soldierSpacing / 2));
                    }
                    for (int j = 0; j < soldierList[i].Count / 2; j++)
                    {
                        //soldierList[i][soldierList[i].Count/2 + j].transform.Translate(Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, -soldierSpacing/2));
                        soldierList[i][soldierList[i].Count / 2 + j].GetComponent<SoldierController>().SetBasePositionAndOffsets(Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, -soldierSpacing / 2));
                    }
                }
                else
                {
                    for (int j = 0; j < soldierList[i].Count / 2 + 1; j++)
                    {
                        //soldierList[i][j].transform.Translate(Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, 0));
                        soldierList[i][j].GetComponent<SoldierController>().SetBasePositionAndOffsets(Vector3.forward * soldierSpacing * j + new Vector3(lineSpacing * -i, 0, 0));
                    }
                    for (int j = 1; j < soldierList[i].Count / 2 + 1; j++)
                    {
                        //soldierList[i][soldierList[i].Count/2 + j].transform.Translate(Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, 0));
                        soldierList[i][soldierList[i].Count / 2 + j].GetComponent<SoldierController>().SetBasePositionAndOffsets(Vector3.forward * soldierSpacing * -j + new Vector3(lineSpacing * -i, 0, 0));
                    }
                }

            }
        }

    }

    public void RefillFirstLine() // transfer soldiers from the second line to the first line
    {
        if(soldierList[0].Count < soldierList[1].Count)
        {
            int missingSoldiers = Mathf.CeilToInt( ((float)soldierList[1].Count - (float)soldierList[0].Count)/2 );
            //Debug.Log(soldierList[1].Count + " - " + soldierList[0].Count + " div = " + Mathf.CeilToInt(((float)soldierList[1].Count - (float)soldierList[0].Count) / 2));
            List<GameObject> soldiersTransfer = soldierList[1].GetRange(soldierList[1].Count - missingSoldiers, missingSoldiers);
            soldierList[1].RemoveRange(soldierList[1].Count - missingSoldiers, missingSoldiers);
            soldierList[0].AddRange(soldiersTransfer);
        }
    }
    
    IEnumerator Fire()
    {
        fireing = true;
        animationChanged.Invoke();

        yield return new WaitForSeconds(2);

        // target is chosen again; solution to problem with target being destroyed by other unit while this coroutine was waiting
        shootingTarget = enemyRegiments.Find(b => Vector3.Distance(transform.position, b.transform.position) == enemyRegiments.Min(a => Vector3.Distance(transform.position, a.transform.position)));

        if (shootingTarget != null)
        {
            if (Vector3.Distance(transform.position, shootingTarget.transform.position) <= shootingRange)
            {
                Ray ray = new Ray(transform.position, shootingTarget.transform.position - transform.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, shootingRange, unitMask))
                {
                    //Debug.DrawLine(ray.origin, hit.point, Color.green, 30);
                    if (hit.collider.gameObject == shootingTarget)
                    {
                        int killsNumber = 0;
                        float shotDifficulty = Mathf.Round(35 - musketInaccuracy * Mathf.Pow(Vector3.Distance(transform.position, shootingTarget.transform.position), 2));

                        readyToFire = false;


                        for (int i = 0; i < soldierCount; i++)
                        {
                            if (Random.Range(1, 101) <= shotDifficulty)
                            {
                                killsNumber++;
                            }
                        }

                        //Debug.Log($"{killsNumber} enemies killed.");
                        shootingTarget.GetComponent<RegimentController>().DealDamage(killsNumber, true);

                        foreach (GameObject soldier in soldierList[0])
                        {
                            soldier.GetComponent<SoldierController>().Shoot();
                        }

                        yield return new WaitForSeconds(2);
                    }
                }

            }
        } 

        fireing = false;
        animationChanged.Invoke();
    }

    public void DealDamage(int killsNumber, bool applyReduction)
    {
        if (applyReduction)
        {
            killsNumber = killsNumber - (int)(killsNumber * actualDamageReduction);
        }


        if (soldierCount < killsNumber)
        {
            killsNumber = soldierCount;
            soldierCount = 0;
        }
        else
        {
            soldierCount -= killsNumber;
        }

        //Debug.Log(soldierList[0].Count());
        for (int i = 0; i < killsNumber; i++)
        {
            (int x, int y) = GetRandomSoldier();
            soldierList[x][y].GetComponent<SoldierController>().Die();
            soldierList[x].RemoveAt(y);
        }

        if (soldierCount == 0)
        {
            DestroyRegiment();
        }


        //Debug.Log((float)soldierCount / (float)startingSoldierCount);

        morale -= killsNumber*2;
        if (morale < 0)
        {
            morale = 0;
        }
        SetMaxMorale();

        if ((float)soldierCount/(float)startingSoldierCount < 0.5f)
        {
            if (Random.Range(1, 101) > morale)
            {
                TestMorale();
            } 
        }

        if (!fleeing)
        {
            StartReformLine();
        }
        
        if (playable)
        {
            regimentIcon.GetComponent<IconController>().UpdateCompanyIcon();
        }

        //Debug.Log(soldierList[0].Count());
    }

    private (int, int) GetRandomSoldier()
    {
        int countAll = 0;
        for (int i = 0; i < soldierList.Count; i++)
        {
            countAll += soldierList[i].Count();
        }

        int randomIndex = Random.Range(0, countAll);

        for (int i = 0; i < soldierList.Count; i++)
        {
            if (randomIndex < soldierList[i].Count())
            {
                return (i, randomIndex);
            }
            else
            {
                randomIndex -= soldierList[i].Count();
            }
        }
        
        return (-1, -1);
    }

    IEnumerator Reload() // to stop this coroutine use StopReload method
    {
        reloading = true;
        animationChanged.Invoke();

        float reloadTime = 60.0f / (reloadRate * Random.Range(0.9f, 1.1f));

        foreach (List<GameObject> line in soldierList)
        {
            foreach (GameObject soldier in line)
            {
                soldier.GetComponent<SoldierController>().AdjustReloadAnimatorSpeed(reloadTime);
            }
        }

        yield return new WaitForSeconds(reloadTime);
        readyToFire = true;

        reloading = false;
        animationChanged.Invoke();
    }

    public void StopReload()
    {
        StopCoroutine("Reload");
        reloading = false;
        animationChanged.Invoke();
    }

    public void StopFireing()
    {
        StopCoroutine("Fire");
        fireing = false;
        animationChanged.Invoke();
    }

    public void DestroyRegiment()
    {
        rtsController.removeCompany(gameObject, side);

        StopAllCoroutines();

        if (playable)
        {
            Destroy(regimentIcon);
        }

        GetComponentInChildren<DropFlag>().drop = true;

        transform.DetachChildren(); // in practice: detach flag, camera (if camera is attached) and soldiers (if some survived)
        
        // give new (now: global) offset to survivors, to make them run further away
        foreach (List<GameObject> line in soldierList)
        {
            foreach (GameObject soldier in line)
            {
                //if (side == Side.Red)
                //{
                //    soldier.GetComponent<SoldierController>().xOffset = -mapBoundryX - 100;
                //}
                //else if (side == Side.Blue)
                //{
                //    soldier.GetComponent<SoldierController>().xOffset = mapBoundryX + 100;
                //}
                soldier.GetComponent<SoldierController>().xOffset = -mapBoundryX - 200;
                soldier.GetComponent<SoldierController>().zOffset = soldier.GetComponent<SoldierController>().transform.position.z;
                soldier.GetComponent<SoldierController>().speed = quickMarchSpeed;
            }
        }


        if (rtsController.selectedRegiments.Contains(gameObject))
        {
            rtsController.selectedRegiments.Remove(gameObject);
        }

        regimentIsBeingDestroyed.Invoke();
        regimentIsBeingDestroyed.RemoveAllListeners();

        Destroy(gameObject);
    }

    private void TestMorale()
    {
        if (!fleeing)
        {
            if (Random.Range(1, 101) > morale)
            {
                fleeing = true;
                StopReload();
                StopFireing();
                //Debug.Log("Morale failed!");

                foreach (List<GameObject> line in soldierList)
                {
                    foreach (GameObject soldier in line)
                    {
                        soldier.GetComponent<SoldierController>().Flee();
                    }
                }

                if (playable)
                {
                    regimentIcon.GetComponent<IconController>().typeSymbolUI.color = Color.white;
                }

                StartCoroutine(TryToReorganise());
            }
            else
            {
                //Debug.Log("Morale passed.");
            }
        }
    }

    void SetMaxMorale()
    {
        if ((float)soldierCount / (float)startingSoldierCount < 0.2f)
        {
            maxMorale = 30;
        }
        else if ((float)soldierCount / (float)startingSoldierCount < 0.5f)
        {
            maxMorale = 70;
        }        
        else if ((float)soldierCount / (float)startingSoldierCount < 0.7f)
        {
            maxMorale = 90;
        }
    }

    IEnumerator TryToReorganise()
    {
        while (fleeing)
        {
            yield return new WaitForSeconds(Random.Range(10.0f, 35.0f));

            if (Random.Range(1, 101) <= morale/2)
            {
                //fleeing = false;
                //Debug.Log("Reorganised!");
                fleeing = false;
                walking = false;
                afterReorganisation = true;

                animationChanged.Invoke();

                task.taskType = TaskType.HoldPosition;
                task.destination.x = transform.position.x;
                // task.destination.y should remain unchanged (as unit could have reorganised beetwen lines and could need to return to line)

                StartCoroutine(RemoveAfterReorganisationState());
                StartReformLine();
                foreach (List<GameObject> line in soldierList)
                {
                    foreach (GameObject soldier in line)
                    {
                        soldier.GetComponent<SoldierController>().Reorganise();
                    }
                }


                if (playable) // TODO: make this a method in IconController
                {
                    if (side == Side.Red)
                    {
                        regimentIcon.GetComponent<IconController>().typeSymbolUI.color = Color.red;
                    }
                    else if (side == Side.Blue)
                    {
                        regimentIcon.GetComponent<IconController>().typeSymbolUI.color = Color.blue;
                    }
                }

            }
            else
            {
                //Debug.Log("Failed to reorganise.");
            }
        }
        
    }

    IEnumerator RemoveAfterReorganisationState()
    {
        yield return new WaitForSeconds(4.0f);
        afterReorganisation = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == unitLayer) // colliding with unit
        {
            if (other.gameObject.GetComponent<RegimentController>().side == side) // colliding with friendly unit
            {
                //Debug.Log("Greetings!");
                disorganised = true;
                StopReload();
                StopFireing();

                speed = 1.0f;

                if (playable)
                {
                    regimentIcon.GetComponent<IconController>().UpdateCompanyIcon();
                }
            }
            else // colliding with enemy unit
            {
                //Debug.Log("Fix bayonets!");

                if (Vector3.Dot(transform.right, other.gameObject.transform.position - transform.position) > 0)
                {
                    meeleTargetFront = other.gameObject;
                }
                else
                {
                    meeleTargetBack = other.gameObject;
                }

                meele = true;
                StopReload();
                StopFireing();

                other.gameObject.GetComponent<RegimentController>().regimentIsBeingDestroyed.AddListener(StopMeele);

            }
        }
        else if (other.gameObject.layer == terrainLayer)
        {
            //Debug.Log("A tree!");

            (speedModifier, actualDamageReduction) = other.gameObject.GetComponent<TerrainModifiers>().GetModifiers(companyType);
            //speedModifier = other.gameObject.GetComponent<TerrainModifiers>().speedModifier;
            //damageReduction = other.gameObject.GetComponent<TerrainModifiers>().damageReduction;
        }

    }

    IEnumerator Melee()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            if (meele)
            {
                int divisor;

                if (meeleTargetFront && meeleTargetBack) 
                {
                    divisor = 20;
                }
                else
                {
                    divisor = 10;
                }



                if (meeleTargetFront)
                {
                    int killsNumber = 0;

                    //Debug.Log(name + " attack front!");

                    for (int i = 0; i < soldierCount / divisor; i++)
                    {
                        if (Random.Range(1, 101) <= meleeSkill)
                        {
                            killsNumber++;
                        }
                    }

                    //Debug.Log($"{killsNumber} enemies killed.");
                    meeleTargetFront.GetComponent<RegimentController>().DealDamage(killsNumber, false);
                }

                if (meeleTargetBack)
                {
                    int killsNumber = 0;

                    //Debug.Log(name + " attack back!");

                    for (int i = 0; i < soldierCount / divisor; i++)
                    {
                        if (Random.Range(1, 101) <= meleeSkill)
                        {
                            killsNumber++;
                        }
                    }

                    //Debug.Log($"{killsNumber} enemies killed.");
                    meeleTargetBack.GetComponent<RegimentController>().DealDamage(killsNumber, false);
                }

            }

        }
    }

    private void StopMeele()
    {
        meele = false;
        animationChanged.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == unitLayer) // colliding with unit
        {
            if (other.gameObject.GetComponent<RegimentController>().side == side) // colliding with friendly unit
            {
                // most of the stuff connected to trigger is handled in OnTriggerEnter
                disorganised = true;
                speed = 1.0f;
            }
            else // colliding with enemy unit
            {
                meele = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Collider collider = GetComponent<Collider>();

        if (other.gameObject.layer == unitLayer) // colliding with unit
        {
            if (other.gameObject.GetComponent<RegimentController>().side == side) // colliding with friendly unit
            {
                //Debug.Log("Bye!");
                disorganised = false;
                QuickMarch();
                if (playable)
                {
                    regimentIcon.GetComponent<IconController>().UpdateCompanyIcon();
                }
            }
            else // colliding with enemy unit
            {
                if (Vector3.Dot(transform.right, other.gameObject.transform.position - transform.position) > 0)
                {
                    meeleTargetFront = null;
                }
                else
                {
                    meeleTargetBack = null;
                }

                meele = false;
                animationChanged.Invoke();
            }
        }
        else if (other.gameObject.layer == terrainLayer)
        {
            speedModifier = 1;
            actualDamageReduction = baseDamageReduction;
        }
    }

    public void QuickMarch()
    {
        if (!disorganised)
        {
            speed = quickMarchSpeed;
        }
    }

    public void MarchMarch()
    {
        if (!disorganised && stamina > 0)
        {
            speed = marchMarchSpeed;
        }
    }

    public void HoldFire()
    {
        holdFire = !holdFire;
    }

    IEnumerator CalculateStamina()
    {
        while (true)
        {
            if (task.taskType == TaskType.HoldPosition)
            {
                stamina += 1;                
            }
            else
            {
                stamina -= (int)speed - (int)quickMarchSpeed + 5;
            }
            stamina = Mathf.Clamp(stamina, 0, 100);
            if (stamina == 0)
            {
                QuickMarch();
            }


            

            if (morale < maxMorale)
            {
                morale += 1;
            }

            if (playable)
            {
                regimentIcon.GetComponent<IconController>().UpdateCompanyIcon();
            }
            
            yield return new WaitForSeconds(1.0f);
        }

    }

}
