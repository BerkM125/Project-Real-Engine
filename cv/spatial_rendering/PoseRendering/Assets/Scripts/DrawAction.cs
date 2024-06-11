/*
 * DrawAction.cs - Berkan Mertan
 * Script that handles all drawing of attacks, actions, movement
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawAction
{
    // DrawAttack is treated as an entirely static class except for the fact that it itself
    // is not static. This is because I needed to extend the MonoBehavior class for manipulation
    // of the game on the main unity thread.
    public class DrawAttack : MonoBehaviour
    {
        // Static cooldown indicators
        public static bool BeamCooldownInitiated = false;
        public static bool RedCooldownInitiated = false;

        // Static prefabs for instantiating attacks
        public static GameObject beamPrefab = GameObject.Find("BEAMTYPE");
        public static GameObject redPrefab = GameObject.Find("REDTYPE");
        public static GameObject POSTPROCESSOR = GameObject.Find("PostProcessing");

        // Static SFX sources
        public static AudioSource purpleSource = GameObject.Find("BEAMTYPE").GetComponent<AudioSource>();
        public static AudioSource redLoadUp = GameObject.Find("REDTYPE").GetComponents<AudioSource>()[0];
        public static AudioSource redBlast = GameObject.Find("REDTYPE").GetComponents<AudioSource>()[1];

        // Static post-processing Bloom component
        private static Bloom bloom;

        // Co-routine invocable BEAM draw function
        public static IEnumerator Beam(Vector3 location, Vector3 direction)
        {
            // Prevent another firing of a beam if cooldown isn't over yet
            if (BeamCooldownInitiated) yield break;
            BeamCooldownInitiated = true;

            GameObject beamFigure = Instantiate(beamPrefab);
            beamFigure.transform.position = location;
            
            purpleSource.Play();

            // Beam firing
            for (int zGrowth = 0; zGrowth < 30; zGrowth++)
            {
                // Cylinder origin is at the CENTER, so we have to translate the cylinder
                // along the ray as we scale it with ray length
                beamFigure.transform.position += Vector3.Scale(new Vector3(0, 0, zGrowth*0.5f), direction);
                beamFigure.transform.localScale += new Vector3(0, zGrowth*0.5f, 0);

                // XP update for this move
                StatusManagement.XP += 0.3f;
                StatusManagement.RenderBars();

                // Slow the drawing a bit
                yield return new WaitForSeconds(0.02f);
            }

            // Keep alive for a bit, then destroy
            yield return new WaitForSeconds(0.85f);
            Destroy(beamFigure);

            // Cooldown over, can go for another attack
            BeamCooldownInitiated = false;
        }

        // Co-routine invocable RED blast (inspired by jjk)
        public static IEnumerator Red(Vector3 location, Vector3 direction)
        {
            // Prevent another firing of the RED technique if the cooldown isn't over
            if (RedCooldownInitiated) yield break;
            RedCooldownInitiated = true;

            // Instantiate the orb
            GameObject redFigure = Instantiate(redPrefab);
            redFigure.transform.position = location;

            // Add the global BLOOM effect for distortion
            POSTPROCESSOR.GetComponent<Volume>().profile.TryGet(out bloom);
           
            // CHARGE UP SFX + DISTORTION...
            redLoadUp.Play();
            for (int bGrowth = 0; bGrowth < 20; bGrowth++)
            {
                bloom.intensity.value += 0.3f;
                yield return new WaitForSeconds(0.02f);
            }
            yield return new WaitForSeconds(0.35f);
            bloom.intensity.value = 0.1f;
            
            // BLAST!!!
            redBlast.Play();

            // Grow the orb and send it flying at the same time
            for (int zGrowth = 0; zGrowth < 30; zGrowth++)
            {
                redFigure.transform.position += Vector3.Scale(new Vector3(0, 0, zGrowth * 0.5f), direction);
                redFigure.transform.localScale += new Vector3(zGrowth * 0.25f, zGrowth * 0.25f, zGrowth * 0.25f);

                // XP update for this move
                StatusManagement.XP += 0.5f;
                StatusManagement.RenderBars();

                yield return new WaitForSeconds(0.02f);
            }

            // Keep alive for a bit, then destroy
            yield return new WaitForSeconds(0.85f);
            Destroy(redFigure);

            // Cooldown over, can go for another attack
            RedCooldownInitiated = false;
        }
    }
}
