using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Health : MonoBehaviour
{
    [HideInInspector]
    public float currentHealth;
    [Tooltip("Max health")]
    public float maxHealth = 3f;
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void DealDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Death();
    }
    [Tooltip("Events which should be called on death, should be set up in the editor")]
    public UnityEvent OnDeath; 

    void Death()
    {
        OnDeath.Invoke();
    }
}
