using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResetLevelOnDeath : MonoBehaviour {
    [SerializeField] GameObject loadingScreen;
    [SerializeField] DungeonCreator dungeonCreator;
    [SerializeField] Image imgLoadingWheel;


    private void OnDestroy() {

    }

    IEnumerator ShowAndHideLoadingScreen() {

        loadingScreen.SetActive(true);
        Time.timeScale = 0f;

        //Show and lerp big        
        float time = 0f;
        while (time < 0.25f) {
            time += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(time / 0.25f);
            loadingScreen.transform.localScale = Vector3.Lerp(new Vector3(0.001f, 0.001f, 0.001f), new Vector3(1, 1, 1), normalizedTime);
            yield return null;
        }

        //Wait 5 seconds and do loading wheel
        time = 0f;
        while (time < 5f) {
            time += Time.unscaledDeltaTime;
            imgLoadingWheel.fillAmount = time / 5f;
            yield return null;
        }


        //Lerp small and hide
        time = 0f;
        while (time < 0.25f) {
            time += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(time / 0.25f);
            loadingScreen.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0.001f, 0.001f, 0.001f), normalizedTime);
            yield return null;
        }

        loadingScreen.SetActive(false);
        Time.timeScale = 1f;
    }
}
