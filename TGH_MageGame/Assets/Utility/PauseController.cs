using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour {
    //**PROPERTIES**
    [Header("Pause Menu Refernces")]
    [SerializeField] RectTransform pauseMenu;
    [SerializeField] AudioSource pauseMenuAudio;
    [SerializeField] AudioClip pauseMenuClip;
    //
    [Header("Pause Menu Properties")]
    [SerializeField] float openCloseSpeed;
    [SerializeField] bool isPaused;
    [SerializeField] bool isAnimationHappening;
    //    
    ActionAsset actionAsset;

    //**FIELDS**
    public bool IsPaused { get => isPaused; set => isPaused = value; }

    //**UNITY METHODS**
    private void Awake() {

        //Initialize
        isPaused = false;
        isAnimationHappening = false;
        actionAsset = new ActionAsset();

        //Set pause menu to inactive
        //pauseMenu.gameObject.SetActive(false);

        //Define Input action callbacks
        actionAsset.Player.OpenPauseMenu.performed += ShowHidePauseMenuWrapper;
    }
    //
    private void OnEnable() {
        //Turn on action assets
        actionAsset.Player.Enable();
    }
    //
    private void OnDisable() {
        //Turn off action assets
        actionAsset.Player.Disable();
    }


    //**UTILITY METHODS**
    public void ShowHidePauseMenu() {
        if (!isAnimationHappening && SceneManager.GetActiveScene().name != "Splash") {
            isAnimationHappening = true;

            if (isPaused) {
                pauseMenuAudio.clip = pauseMenuClip;
                pauseMenuAudio.Play();
            }
            StartCoroutine(ShrinkOrGrowPauseMenu(isPaused ? 0.001f : 1));
        }
    }

    public void ShowHidePauseMenuWrapper(InputAction.CallbackContext context) {
        ShowHidePauseMenu();
    }

    public void MainMenu() {
        pauseMenuAudio.clip = pauseMenuClip;
        pauseMenuAudio.Play();
        StartCoroutine(GoToMainMenu());
    }

    public void Exit() {
        pauseMenuAudio.clip = pauseMenuClip;
        pauseMenuAudio.Play();
        Application.Quit();
        StartCoroutine(ExitGame());
    }


    //**COROUTINES**
    IEnumerator ShrinkOrGrowPauseMenu(float targetScale) {

        Vector3 startScale = pauseMenu.transform.localScale;
        Vector3 targetVector = new Vector3(targetScale, targetScale, targetScale);

        float time = 0f;

        if (isPaused) {
            actionAsset.Player.Enable();
            isPaused = false;
            Cursor.visible = false;

            //Lerp small            
            while (time < openCloseSpeed) {
                time += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(time / openCloseSpeed);
                pauseMenu.transform.localScale = Vector3.Lerp(startScale, targetVector, normalizedTime);
                yield return null;
            }

            // Hide the pause menu
            pauseMenu.gameObject.SetActive(false);
            Time.timeScale = 1f; // Resume the game time

        }
        else {

            // Show the pause menu
            actionAsset.Player.Disable();
            isPaused = true;
            pauseMenu.gameObject.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = true;

            //Lerp big
            while (time < openCloseSpeed) {
                time += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(time / openCloseSpeed);
                pauseMenu.transform.localScale = Vector3.Lerp(startScale, targetVector, normalizedTime);
                yield return null;
            }
        }

        isAnimationHappening = false;
    }
    //
    IEnumerator GoToMainMenu() {


        Debug.Log("Here");
        yield return new WaitForSeconds(1.613f);
        Debug.Log("Here");

        //Load Title Scene
        SceneManager.LoadScene("Splash");
    }

    IEnumerator ExitGame() {
        yield return new WaitForSeconds(1.613f);
        //Exit game
        Application.Quit();
    }
}


