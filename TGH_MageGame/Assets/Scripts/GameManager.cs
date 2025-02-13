using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [SerializeField] Canvas hud;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] Transform player;
    [SerializeField] GameObject introOverlayPanel;
    [SerializeField] GameObject outroOverlayPanel;
    [SerializeField] AudioClip fireFX;
    [SerializeField] AudioClip levelSound;

    public bool playOutro;
    bool outroPlayed = false;

    Vector3 playerPivot;
    Vector3 introOverlayPanelHiddenPos = new Vector3(0, -500, 0);
    Vector3 outroOverlayPanelHiddenPos = new Vector3(0, -1500, 0);


    //DEV ONLY - REMOVE BEFORE BUILD
    Transform mousePositionObject;
    [Header("Bugs / Issues")]
    [SerializeField] private List<string> knownBugs = new List<string>();

    private void Awake() {
        //DEV ONLY - REMOVE BEFORE BUILD - setup debug object
        mousePositionObject = new GameObject().transform;
        mousePositionObject.name = "MousePosObject";

        foreach (string bug in knownBugs) {
            if (bug != "") {
                Debug.LogWarning(bug);
            }
        }

        //DEMO OVERLAY CODE
        //set sound
        Camera cam = Camera.main;
        cam.GetComponent<AudioSource>().resource = fireFX;
        cam.GetComponent<AudioSource>().Play();

        //get tmp assets
        TextMeshProUGUI[] introTMPs = introOverlayPanel.GetComponentsInChildren<TextMeshProUGUI>();

        //set intro panel pos
        introOverlayPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        //hide text
        for (int i = 0; i < introTMPs.Length; i++) {
            introTMPs[i].color = new Color(1, 1, 1, 0);
        }

        StartCoroutine(PlayIntroOverlay(introTMPs));

    }

    private void Update() {
        MoveProjectileSpawn();

        if (!outroPlayed) {
            if (playOutro) {
                outroPlayed = true;
                StartCoroutine(PlayOutroOverlay());
            }
        }
    }

    private void OnApplicationFocus(bool focus) {
        Cursor.visible = false;
    }

    void MoveProjectileSpawn() {

        //get mouse input position
        Vector3 mousePos = Mouse.current.position.ReadValue();

        //convert mouse input to point in world 
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(Camera.main.transform.position.z)));

        //get position in center of player model
        playerPivot = new Vector3(player.position.x, player.position.y + 1.162f, 0);

        //setup ray 
        Ray ray = new Ray(playerPivot, new Vector3(worldPos.x, worldPos.y, 0) - playerPivot);

        //spell spawn point offset from centermass of player        
        float offset = 1.25f;//DEFAULT IS .783f ONCE SPELL COLLISION DONE

        //move projectile spawn point
        projectileSpawn.transform.position = ray.GetPoint(offset);

        //set debug object to world pos of mouse
        mousePositionObject.position = new Vector3(worldPos.x, worldPos.y, 0);

        //DEV ONLY - REMOVE BEFORE BUILD - draw ray
        //Debug.DrawRay(centerMass, debugObject.position - centerMass, Color.red);
    }


    public Vector3 GetMousePositionInWorldSpace() {
        return mousePositionObject.position;
    }

    public Vector3 GetPlayerPivot() {
        return playerPivot;
    }

    public void Quit() {
        Application.Quit();
    }

    public void LoadMainMenu() {
        SceneManager.LoadScene("Splash");
    }

    IEnumerator PlayIntroOverlay(TextMeshProUGUI[] TMPs) {
        //stop player input
        PlayerController pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        pc.FreezePhysics = true;


        //First text
        float elapsedTime = 0f;
        float duration1 = 8;
        Color originalColor = TMPs[0].color;


        while (elapsedTime < duration1) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration1);
            TMPs[0].color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        TMPs[0].color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        //Second text
        elapsedTime = 0f;
        float duration2 = 12;
        originalColor = TMPs[1].color;
        while (elapsedTime < duration2) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration2);
            TMPs[1].color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        TMPs[1].color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        Camera cam = Camera.main;
        cam.GetComponent<AudioSource>().resource = levelSound;
        cam.GetComponent<AudioSource>().Play();

        pc.FreezePhysics = false;
        introOverlayPanel.GetComponent<RectTransform>().anchoredPosition = introOverlayPanelHiddenPos;
    }

    IEnumerator PlayOutroOverlay() {
        //stop player input
        PlayerController pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        pc.FreezePhysics = true;

        //First text
        float elapsedTime = 0f;
        float duration = 1;
        Image panelImage = outroOverlayPanel.GetComponent<Image>();
        TextMeshProUGUI outroTMP = outroOverlayPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Image logoImage = outroOverlayPanel.transform.GetChild(1).GetComponent<Image>();
        Image mainMenuButtonImage = outroOverlayPanel.transform.GetChild(2).GetComponent<Image>();
        TextMeshProUGUI buttonTMP = mainMenuButtonImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        Color panelOriginalColor = panelImage.color;
        Color textOriginalColor = outroTMP.color;
        Color logoOriginalColor = logoImage.color;
        Color buttonOriginalColor = mainMenuButtonImage.color;
        Color buttonTMPOriginalColor = buttonTMP.color;

        panelImage.color = new Color(panelOriginalColor.r, panelOriginalColor.g, panelOriginalColor.b, 0);
        outroTMP.color = new Color(textOriginalColor.r, textOriginalColor.g, textOriginalColor.b, 0);
        logoImage.color = new Color(logoOriginalColor.r, logoOriginalColor.g, logoOriginalColor.b, 0);
        mainMenuButtonImage.color = new Color(logoOriginalColor.r, logoOriginalColor.g, logoOriginalColor.b, 0);
        buttonTMP.color = new Color(logoOriginalColor.r, logoOriginalColor.g, logoOriginalColor.b, 0);

        //move panel intoposition
        outroOverlayPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            panelImage.color = new Color(panelOriginalColor.r, panelOriginalColor.g, panelOriginalColor.b, alpha);
            outroTMP.color = new Color(textOriginalColor.r, textOriginalColor.g, textOriginalColor.b, alpha);
            logoImage.color = new Color(logoOriginalColor.r, logoOriginalColor.g, logoOriginalColor.b, alpha);
            mainMenuButtonImage.color = new Color(buttonOriginalColor.r, buttonOriginalColor.g, buttonOriginalColor.b, alpha);
            buttonTMP.color = new Color(buttonTMPOriginalColor.r, buttonTMPOriginalColor.g, buttonTMPOriginalColor.b, alpha);
            yield return null;
        }
    }
}

