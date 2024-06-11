/*
 * BeamCollider.cs - Berkan Mertan
 * Collision script for the PHOTON BEAM attack.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamCollider : MonoBehaviour
{
    private bool attackHit = false;
    void OnTriggerEnter(Collider collision)
    {
        // We don't want the collision to trigger OVER AND OVER again
        if (attackHit) return;
        attackHit = true;

        if (collision.transform != null &&
            collision.transform.parent != null &&
            collision.transform.parent.parent != null)
        {
            if (collision.transform.parent.parent.tag == "Player")
            {
                StatusManagement.HEALTH -= 10;
                StatusManagement.RenderBars();
            }
        }
    }
}
