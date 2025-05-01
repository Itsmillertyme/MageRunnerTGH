using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour {

    [SerializeField] AudioSource menuAudio;
    [SerializeField] AudioClip startGameClip;
    [SerializeField] MusicManager musicManager;

    private void Awake() {
        musicManager.SwitchPlaylist(0); // Switch to the splash screen playlist
    }

    public void StartNewGame() {

        menuAudio.clip = startGameClip;
        menuAudio.Play();
        StartCoroutine(BeginGame());
    }


    IEnumerator BeginGame() {

        yield return new WaitForSeconds(1.613f);

        //Load level 1
        SceneManager.LoadScene("Level1_Dungeon");
        musicManager.SwitchPlaylist(1); // Switch to the first playlist (level 1 music)


    }
}
