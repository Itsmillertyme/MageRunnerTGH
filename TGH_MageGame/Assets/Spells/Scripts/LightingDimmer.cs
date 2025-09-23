using UnityEngine;

public class LightingDimmer : MonoBehaviour
{
    private Light[] lights;
    private float[] defaultLightIntensities;

    void Start()
    {
        lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        StoreDefaultLightData();
    }

    private void StoreDefaultLightData()
    {
        defaultLightIntensities = new float[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            defaultLightIntensities[i] = lights[i].intensity;
        }
    }

    public void DimLights()
    {
        // TURN LIGHTS INTENSITY DOWN OVER TIME THEN BACK UP. SYNC TIMING WITH LIGHTNING SPELL
    }
}