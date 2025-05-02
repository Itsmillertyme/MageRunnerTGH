using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RuneMenuController : MonoBehaviour {

    //**PROPERTIES**
    [Header("Stat Properties")]
    [SerializeField] float statValue;
    [SerializeField] float statStep;

    [Header("GUI References")]
    [SerializeField] GameObject[] runeOrbs;
    [SerializeField] Button increaseButton;
    [SerializeField] Button decreaseButton;

    [Header("Misc")]
    [SerializeField] UnityEvent runeLevelRaised;

    int statLevel;

    //**FIELDS**
    public int StatLevel { get => statLevel; set => statLevel = value; }
    public float StatValue { get => statValue; set => statValue = value; }
    public float StatStep { get => statStep; set => statStep = value; }

    //**UNITY METHODS**
    private void Awake() {
        //Initialize        
        statLevel = 1;

        UpdateRuneOrbs();
    }

    //**UTILITY METHODS**
    public void IncreaseLevel() {
        if (statLevel < 10) {
            statLevel++;
            statValue += statStep;
            UpdateRuneOrbs();

            runeLevelRaised.Invoke();
        }
    }
    //
    public void DecreaseLevel() {
        if (statLevel > 1) {
            statLevel--;
            statValue -= statStep;
            UpdateRuneOrbs();
        }
    }
    //
    private void UpdateRuneOrbs() {

        //Turn on appropriate number of orbs
        for (int i = 0; i < runeOrbs.Length; i++) {
            if (i < statLevel) {
                runeOrbs[i].SetActive(true);
            }
            else {
                runeOrbs[i].SetActive(false);
            }
        }

        ////Turn on lightning bolt
        //for (int i = 0; i < statLevel - 1; i++) {
        //    GameObject boltObj = Instantiate(runeBolt.gameObject);
        //    LightningBolt2D.LightningBolt2D bolt = boltObj.GetComponent<LightningBolt2D.LightningBolt2D>();

        //    bolt.isPlaying = true;

        //    RectTransform boltRect = boltObj.GetComponent<RectTransform>();
        //    boltRect.SetParent(runeOrbs[i].transform.parent);
        //    boltRect.SetSiblingIndex(0);
        //    boltRect.anchoredPosition = new Vector3(0, 0, 0);
        //    boltRect.offsetMin = new Vector2(0, 0);
        //    boltRect.offsetMax = new Vector2(0, 0);
        //}

        //Disable the increase button at lvl 10
        if (statLevel == 10) {
            increaseButton.interactable = false;
        }
        else {
            increaseButton.interactable = true;
        }

        // Disable the decrease button at lvl 1
        if (statLevel == 1) {
            decreaseButton.interactable = false;
        }
        else {
            decreaseButton.interactable = true;
        }
    }
}
