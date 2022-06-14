using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifiers : MonoBehaviour
{
    [SerializeField] float infantrySpeedModifier = 1; // 0 - inf, speed multiplier
    [SerializeField] float infantryDamageReduction = 0; // 0 - 1, fraction of kills that will be ignored
    [SerializeField] float skrimishersSpeedModifier = 1;
    [SerializeField] float skrimishersDamageReduction = 0;


    public (float, float) GetModifiers(CompanyType companyType)
    {
        if (companyType == CompanyType.Infantry)
        {
            return (infantrySpeedModifier, infantryDamageReduction);
        }
        else if (companyType == CompanyType.Skrimishers)
        {
            return (skrimishersSpeedModifier, skrimishersDamageReduction);
        }
        else
        {
            return (0, 0);
        }
    }
}
