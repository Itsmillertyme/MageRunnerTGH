using UnityEngine;

// TEMP SCRIPT TO SIMULATE SCENE SWITCHING FOR MUSIC MANAGER. REMOVE AFTER MERGE WHEN MM CAN BE INTEGRATED

public class Workaroundformusicmanager : MonoBehaviour
{
    public KeyCode sceneSwitchKey;
    private MusicManager mm;
    private int counter;

    void Awake()
    {
        mm = GetComponent<MusicManager>();
        counter = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(sceneSwitchKey))
        {
            mm.SwitchPlaylist(SimulateSwitchPlaylistScene());
        }
    }

    int SimulateSwitchPlaylistScene()
    {
        if (counter == 0)
        {
            counter = 1;
        }
        else if (counter == 1)
        {
            counter = 0;
        }
        return counter;
    }
}
