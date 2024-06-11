/*
 * StatusManagement.cs - Berkan Mertan
 * Script used to update basic player status GUI, health bar, XP bar, etc.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusManagement : MonoBehaviour
{
    // These should be static since they are universal
    public static float XP = 10.0f;
    public static float HEALTH = 80f;

    // These will be instances of UI
    public static Image HEALTHBAR;
    public static Image XPBAR;

    private void Start()
    {
        HEALTHBAR = GameObject.Find("Health").GetComponent<Image>();
        XPBAR = GameObject.Find("XP").GetComponent<Image>();

        RenderBars();
    }

    // Should be static for universality
    public static void RenderBars()
    {
        XPBAR.fillAmount = XP / 100.0f;
        HEALTHBAR.fillAmount = HEALTH / 100.0f;
    }
}
