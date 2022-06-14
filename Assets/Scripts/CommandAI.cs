using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandAI : MonoBehaviour
{
    public List<GameObject> commandedCompanies;
    public List<GameObject> enemyCompanies;

    public List<GameObject> targetCompanies;

    private RtsController rtsController;
    public Side side = Side.Blue;

    float baseXposition = 0; // basic x coordinate to be hold/conquered

    // Start is called before the first frame update
    void Start()
    {
        rtsController = GameObject.Find("GameManager").GetComponent<RtsController>();

        
        if (side == Side.Red)
        {

        }
        else if (side == Side.Blue)
        {
            commandedCompanies = rtsController.blueRegiments;
            enemyCompanies = rtsController.redRegiments;
        }

        rtsController.startBattleEvent.AddListener(StartingCommands);

    }

    void StartingCommands()
    {
        //Vector3 baseLocation = new Vector3(baseXposition, 0, 0);
        //rtsController.MoveCommand(commandedCompanies, baseLocation, 0);

        foreach (GameObject company in commandedCompanies)
        {
            targetCompanies.Add(enemyCompanies[Random.Range(0, enemyCompanies.Count)]);
        }

        StartCoroutine(GiveCommands());
    }

    IEnumerator GiveCommands()
    {
        while (true)
        {
            //foreach (GameObject company in commandedCompanies)
            //{

            //}

            RandomPursuit();
            //yield return new WaitForSeconds(1.0f);
            yield return new WaitForSeconds(Random.Range(5, 16));
        }   
    }

    void Pursuit()
    {
        List<GameObject> affectedCompanies = new List<GameObject>();

        foreach (GameObject company in commandedCompanies)
        {
            if (Mathf.Abs(enemyCompanies[0].gameObject.transform.position.x - company.transform.position.x) > 50 && !company.GetComponent<RegimentController>().reloading && !company.GetComponent<RegimentController>().fireing)
            {
                affectedCompanies.Add(company);
            }

        }

        //enemyCompanies[0].gameObject.transform.position
        Vector3 targetLocation = enemyCompanies[0].gameObject.transform.position;
        targetLocation = targetLocation + new Vector3(50, 0 , 0);
        rtsController.MoveCommand(affectedCompanies, targetLocation, 0);
    }

    void RandomPursuit()
    {
        List<GameObject> affectedCompanies = new List<GameObject>();

        for (int i = 0; i < commandedCompanies.Count; i++)
        {
            if (targetCompanies[i].gameObject == null) // refresh targets list
            {
                targetCompanies[i] = enemyCompanies[Random.Range(0, enemyCompanies.Count)];
            }

            int dice = Random.Range(1, 11); 
            if (dice == 1) // random charge
            {
                Vector3 targetLocation = targetCompanies[i].gameObject.transform.position;
                targetLocation = targetLocation + new Vector3(-50, 0, 0);
                rtsController.MoveCommand(new List<GameObject> { commandedCompanies[i] }, targetLocation, 0);
                targetCompanies[i].GetComponent<RegimentController>().MarchMarch();
            }
            else if (dice == 2) // random line chage
            {
                if (!commandedCompanies[i].GetComponent<RegimentController>().reloading && !commandedCompanies[i].GetComponent<RegimentController>().fireing)
                {
                    Vector3 targetLocation = commandedCompanies[i].gameObject.transform.position;

                    float changeLine;
                    if (Random.Range(1, 3) == 1)
                    {
                        changeLine = 40;
                    }
                    else
                    {
                        changeLine = -40;
                    }
                    rtsController.MoveCommand(new List<GameObject> { commandedCompanies[i] }, targetLocation, changeLine);
                }
                targetCompanies[i].GetComponent<RegimentController>().QuickMarch();
            }
            else // normal movement
            {
                if (commandedCompanies[i].GetComponent<RegimentController>().disorganised) // try to resolve overlapping companies
                {
                    Vector3 targetLocation = commandedCompanies[i].gameObject.transform.position;
                    targetLocation = targetLocation + new Vector3(Random.Range(-10.0f, 10.0f), 0, 0);
                    rtsController.MoveCommand(new List<GameObject> { commandedCompanies[i] }, targetLocation, 0);
                }
                else if (Mathf.Abs(targetCompanies[i].gameObject.transform.position.x - commandedCompanies[i].transform.position.x) > 50 && !commandedCompanies[i].GetComponent<RegimentController>().reloading && !commandedCompanies[i].GetComponent<RegimentController>().fireing)
                {
                    Vector3 targetLocation = targetCompanies[i].gameObject.transform.position;
                    targetLocation = targetLocation + new Vector3(Random.Range(30, 70), 0, 0);
                    rtsController.MoveCommand(new List<GameObject> { commandedCompanies[i] }, targetLocation, 0);
                }
                targetCompanies[i].GetComponent<RegimentController>().QuickMarch();
            }

        }     

    }
}
