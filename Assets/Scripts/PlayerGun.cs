using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : HitScanFromCamera
{
    [SerializeField] private float damage;
    [SerializeField] private float cost;

    private void Shoot()
    {
        // Try to get a hit
        RaycastHit hit = CastFromSceenCentre();
        // Check the hit for a combat agent
        if (hit.collider)
        {
            if(hit.collider.TryGetComponent<CombatAgent>(out CombatAgent agent))
            {
                agent.TakeDamage(damage);
            }
        }

        // Make them take damage if found
    }

    private void Update()
    {
        if (Input.GetButtonDown("Shoot"))
        {
            Shoot();
        }
    }
}
