using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DrawAction
{
    public class DrawAttack : MonoBehaviour
    {
        public static bool BeamCooldownInitiated = false;
        public static bool RedCooldownInitiated = false;
        public static GameObject beamPrefab = GameObject.Find("BEAMTYPE");
        public static GameObject redPrefab = GameObject.Find("REDTYPE");
        public static GameObject POSTPROCESSOR = GameObject.Find("PostProcessing");

        public static AudioSource purpleSource;
        public static AudioSource redLoadUp;
        public static AudioSource redBlast;

        private static Bloom bloom;

        public static IEnumerator Beam(Vector3 location, Vector3 direction)
        {
            if (BeamCooldownInitiated) yield break;

            BeamCooldownInitiated = true;

            
            GameObject beamFigure = Instantiate(beamPrefab);
            beamFigure.transform.position = location;
            purpleSource = GameObject.Find("BEAMTYPE").GetComponent<AudioSource>();
            purpleSource.Play();
            for (int zGrowth = 0; zGrowth < 30; zGrowth++)
            {
                beamFigure.transform.position += Vector3.Scale(new Vector3(0, 0, zGrowth*0.5f), direction);
                beamFigure.transform.localScale += new Vector3(0, zGrowth*0.5f, 0);

                // XP update for this move
                StatusManagement.XP += 0.3f;
                StatusManagement.RenderBars();

                yield return new WaitForSeconds(0.02f);
            }

            // Keep alive for a bit, then destroy
            yield return new WaitForSeconds(0.85f);
            Destroy(beamFigure);

            // Cooldown over, can go for another attack
            BeamCooldownInitiated = false;
        }

        public static IEnumerator Red(Vector3 location, Vector3 direction)
        {
            if (RedCooldownInitiated) yield break;

            RedCooldownInitiated = true;

            GameObject redFigure = Instantiate(redPrefab);
            redFigure.transform.position = location;

            POSTPROCESSOR.GetComponent<Volume>().profile.TryGet(out bloom);
            redLoadUp = GameObject.Find("REDTYPE").GetComponents<AudioSource>()[0];
            redLoadUp.Play();
            for (int bGrowth = 0; bGrowth < 20; bGrowth++)
            {
                bloom.intensity.value += 0.3f;
                yield return new WaitForSeconds(0.02f);
            }
                
            yield return new WaitForSeconds(0.35f);
            bloom.intensity.value = 0.1f;
            redBlast = GameObject.Find("REDTYPE").GetComponents<AudioSource>()[1];
            redBlast.Play();
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
