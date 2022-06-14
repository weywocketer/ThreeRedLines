using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TreeResizer : MonoBehaviour
{
    //[SerializeField] bool resizeApplied = false;
    //Application.platform == RuntimePlatform.WindowsEditor
    [SerializeField] SpriteRenderer leaves;
    [SerializeField] VegetationType vegetationType;

    // Start is called before the first frame update
    void Start()
    {
    //#if (UNITY_EDITOR)
        //{

        if (vegetationType == VegetationType.Iberia)
        {
            transform.localScale = new Vector3(Random.Range(0.7f, 1.3f), Random.Range(0.7f, 1.3f), 1);
            leaves.color = new Color(0.53f, Random.Range(0.48f, 0.75f), 0.045f);
        }
        else if (vegetationType == VegetationType.Continental)
        {
            transform.localScale = new Vector3(Random.Range(0.9f, 1.5f), Random.Range(0.9f, 1.5f), 1);
            leaves.color = new Color(0.08f, Random.Range(0.35f, 0.65f), 0.08f);
        }

        //leaves.color = new Color(0.08f, Random.Range(0.35f, 0.65f), 0.08f); // Europe
        //leaves.color = new Color(0.53f, Random.Range(0.48f, 0.75f), 0.045f); // Iberia


        //}
        //#endif

    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

}
