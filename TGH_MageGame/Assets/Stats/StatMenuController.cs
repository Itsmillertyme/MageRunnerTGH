using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StatMenuController : MonoBehaviour {

    [SerializeField] RectTransform[] runesRects;
    [SerializeField] GameObject statMenu;



    ActionAsset actionAsset;
    RuneData[] runes;

    [SerializeField] bool isStatMenuOpen = false;
    [SerializeField] bool isSubMenuOpen = false;

    [SerializeField] bool isAnimationHappening = false;

    private void Awake() {
        //Initialize
        actionAsset = new ActionAsset();
        runes = new RuneData[runesRects.Length];

        //build Rune data
        for (int i = 0; i < runesRects.Length; i++) {
            if (runesRects[i] != null) {
                runes[i].rune = runesRects[i];
                runes[i].runeStartPosition = runesRects[i].anchoredPosition;
                runes[i].indexOnSlate = i;
            }
        }

        //Assign event liseners to each rune button
        for (int i = 0; i < runes.Length; i++) {
            if (runes[i].rune != null) {
                //Helpers
                Button runeButton = runes[i].rune.GetComponentInChildren<Button>();
                int temp = i;

                //overlay button listeners
                runeButton.onClick.AddListener(delegate { ShowOrHideAllRunesExceptIndex(temp); });


                //back button listeners
                GameObject backBtnObject = runes[i].rune.GetChild(runes[i].rune.childCount - 1).gameObject;
                Button backButton = backBtnObject.GetComponent<Button>();

                backBtnObject.SetActive(true);
                backButton.onClick.AddListener(delegate { ShowOrHideAllRunesExceptIndex(temp); });
                backBtnObject.SetActive(false);
            }
        }

        //Define Input action callbacks
        actionAsset.Player.OpenStatMenu.performed += ShowOrHideStatMenu;
    }

    private void OnEnable() {
        //Turn on action assets
        actionAsset.Player.Enable();
    }
    //
    private void OnDisable() {
        //Turn off action assets
        actionAsset.Player.Disable();
    }

    public void ShowOrHideAllRunesExceptIndex(int indexToIgnore) {

        for (int i = 0; i < runes.Length; i++) {
            if (runes[i].rune != null) {
                if (!isSubMenuOpen) {
                    if (i != indexToIgnore) {
                        // Fade out and turn off the rune
                        StartCoroutine(FadeRuneAndTurnOffOrOn(runes[i].rune, 0f, 0.25f));
                    }
                    else {
                        //Grow and center rune
                        StartCoroutine(ShrinkOrGrowRune(runes[i], 2f, 0.25f));
                    }
                    Button runeBtn = runes[i].rune.GetComponentInChildren<Button>();
                    runeBtn.interactable = false;
                }
                else {
                    if (i != indexToIgnore) {
                        // Turn on and fade in the rune
                        StartCoroutine(FadeRuneAndTurnOffOrOn(runes[i].rune, 1f, 0.25f));
                    }
                    else {
                        //Shrink and position rune
                        StartCoroutine(ShrinkOrGrowRune(runes[i], 1f, 0.25f));
                    }
                    Button runeBtn = runes[i].rune.GetComponentInChildren<Button>();
                    runeBtn.interactable = true;
                }
            }
        }
    }

    public void ShowOrHideStatMenu(InputAction.CallbackContext context) {

        if (!isAnimationHappening) {
            isAnimationHappening = true;
            StartCoroutine(ShrinkOrGrowStatMenu(isStatMenuOpen ? 0.001f : 1, 0.25f));
        }

    }

    IEnumerator FadeRuneAndTurnOffOrOn(RectTransform element, float targetAlpha, float duration) {

        Image[] runeImages = element.GetComponentsInChildren<Image>();

        float startAlpha = runeImages[0].color.a;
        float time = 0f;

        // Turn on the game object if fading to 1
        if (targetAlpha == 1f) {
            element.gameObject.SetActive(true);
        }

        while (time < duration) {
            time += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(time / duration);
            for (int i = 0; i < runeImages.Length; i++) {
                Image runeImage = runeImages[i];
                Color newColor = runeImage.color;
                newColor.a = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
                runeImage.color = newColor;
            }

            //Color newColor = runeImages.color;
            //newColor.a = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            //runeImage.color = newColor;
            yield return null;
        }

        // Ensure exact final alpha
        for (int i = 0; i < runeImages.Length; i++) {
            Image runeImage = runeImages[i];
            Color finalColor = runeImage.color;
            finalColor.a = targetAlpha;
            runeImage.color = finalColor;
        }
        //Color finalColor = runeImage.color;
        //finalColor.a = targetAlpha;
        //runeImage.color = finalColor;

        // Turn off the game object if fading to 0
        if (targetAlpha == 0f) {
            element.gameObject.SetActive(false);
        }
    }

    IEnumerator ShrinkOrGrowRune(RuneData runeData, float targetScale, float duration) {
        Vector3 startScale = runeData.rune.localScale;
        Vector3 targetVector = new Vector3(targetScale, targetScale, targetScale);
        float time = 0f;

        if (isSubMenuOpen) {
            //hide btns
            for (int i = 1; i < 4; i++) {
                GameObject btn = runeData.rune.transform.GetChild(runeData.rune.transform.childCount - i).gameObject;
                btn.SetActive(false);
            }
        }

        while (time < duration) {
            time += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(time / duration);
            runeData.rune.localScale = Vector3.Lerp(startScale, targetVector, normalizedTime);

            //center
            if (!isSubMenuOpen) {
                //go to center
                runeData.rune.anchoredPosition = Vector2.Lerp(runeData.rune.anchoredPosition, new Vector2(0, 0), normalizedTime);
            }
            else {
                //go to start position
                runeData.rune.anchoredPosition = Vector2.Lerp(runeData.rune.anchoredPosition, runeData.runeStartPosition, normalizedTime);
            }

            yield return null;
        }
        if (!isSubMenuOpen) {
            //show btns
            for (int i = 1; i < 4; i++) {
                GameObject btn = runeData.rune.transform.GetChild(runeData.rune.transform.childCount - i).gameObject;
                btn.SetActive(true);
            }
        }


        // Ensure exact final scale
        runeData.rune.localScale = targetVector;

        isSubMenuOpen = !isSubMenuOpen;
    }

    IEnumerator ShrinkOrGrowStatMenu(float targetScale, float duration) {

        Vector3 startScale = statMenu.transform.localScale;
        Vector3 targetVector = new Vector3(targetScale, targetScale, targetScale);

        float time = 0f;

        if (isStatMenuOpen) {
            isStatMenuOpen = false;
            Cursor.visible = false;

            //Lerp small            
            while (time < duration) {
                time += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(time / duration);
                statMenu.transform.localScale = Vector3.Lerp(startScale, targetVector, normalizedTime);
                yield return null;
            }

            // Hide the stat menu
            statMenu.SetActive(false);
            Time.timeScale = 1f; // Resume the game time

        }
        else {

            // Show the stat menu
            isStatMenuOpen = true;
            statMenu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = true;

            //Lerp big
            while (time < duration) {
                time += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(time / duration);
                statMenu.transform.localScale = Vector3.Lerp(startScale, targetVector, normalizedTime);
                yield return null;
            }
        }

        isAnimationHappening = false;
    }
}

public struct RuneData {
    public RectTransform rune;
    public Vector2 runeStartPosition;
    public int indexOnSlate;
}
