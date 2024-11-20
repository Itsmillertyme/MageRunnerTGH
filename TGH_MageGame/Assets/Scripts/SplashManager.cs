using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour {

    [SerializeField] AudioSource menuAudio;
    [SerializeField] AudioClip startGameClip;

    public void StartNewGame() {

        menuAudio.clip = startGameClip;
        menuAudio.Play();
        StartCoroutine(BeginGame());
    }


    IEnumerator BeginGame() {

        yield return new WaitForSeconds(1.613f);

        //Load level 1
        SceneManager.LoadScene("Level1_Dungeon");

    }
}
