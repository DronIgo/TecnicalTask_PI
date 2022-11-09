using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    //Unfortunaltly I only had time for one type of bonus
    public enum BonusType { 
        POWERPILL,
    };
    [Tooltip("Bonus type")]
    public BonusType type = BonusType.POWERPILL;
    [Tooltip("Duration of Power Pill effect")]
    public float powerPillDuration = 3f;
}
