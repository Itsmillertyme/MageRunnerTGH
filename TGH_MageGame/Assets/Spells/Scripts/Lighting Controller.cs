using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LightingController : MonoBehaviour
{
    private float[] defaultLightIntensities;
    private List<Light> selectedLights;

    private void Awake()
    {
        selectedLights = new();
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        StoreDefaultLightData(allLights);
    }

    private void StoreDefaultLightData(Light[] allLightsInput)
    {
        int projectileLayerIndex = LayerMask.NameToLayer("Player Projectile");

        // FILTER OUT LIGHTS RELATED TO PROJECTILES AND ADD THEM TO A LIST
        foreach (Light light in allLightsInput)
        {
            if (light.gameObject.layer != projectileLayerIndex)
            {
                selectedLights.Add(light);
            }
        }

        // CREATE AN ARRAY TO HOLD INTENSITIES OF THE SAME LENGTH AS SELECTED LIGHTS
        defaultLightIntensities = new float[selectedLights.Count];

        // SET INTENSITIES
        for (int i = 0; i < selectedLights.Count; i++)
        {
            defaultLightIntensities[i] = selectedLights[i].intensity;
        }
    }

    public void DimLights(float dimTime, float dimScale)
    {
        StartCoroutine(DimThenUndim(dimTime, dimScale));
    }

    private IEnumerator DimThenUndim(float totalDimtime, float newDimScale)
    {
        // DIM
        for (int i = 0; i < selectedLights.Count; i++)
        {
            selectedLights[i].intensity = defaultLightIntensities[i] * newDimScale;
        }

        // WAIT
        yield return new WaitForSeconds(totalDimtime);

        // UNDIM
        for (int i = 0; i < selectedLights.Count; i++)
        {
            selectedLights[i].intensity = defaultLightIntensities[i];
        }
    }
}