using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatAgent : MonoBehaviour
{
    // How much health the agent currently has
    [SerializeField] protected float healthCurrent;
    // The largest amount of health allowed
    [SerializeField] protected float healthMax;

    public void TakeDamage(float damage)
    {
        healthCurrent -= damage;
        if(healthCurrent <= 0)
        {
            healthCurrent = 0;
            EndOfLife();
        }
    }

    public void Heal(float heal)
    {
        healthCurrent = Mathf.Clamp(healthCurrent + heal, 0, healthMax);
    }

    // Abstract method = must be filled in by child class
    protected abstract void EndOfLife();

}
