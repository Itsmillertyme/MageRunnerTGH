using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    //**PROPERTIES**
    [Header("Component References")]
    [SerializeField] Canvas hud;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] Transform player;
    [SerializeField] GameObject introOverlayPanel;
    [SerializeField] GameObject outroOverlayPanel;
    [SerializeField] AudioClip fireFX;
    [SerializeField] AudioClip levelSound;
    [SerializeField] RectTransform crosshairRect;
    [Header("Scritable Object References")]
    [SerializeField] LevelEnemies levelEnemies;
    [SerializeField] LevelDecorations levelDecorations;
    [Header("Demo Settings")]
    [SerializeField] bool playIntro;
    [SerializeField] bool playOutro;
    //
    bool outroPlayed = false;
    //
    Vector3 playerPivot;
    Vector3 introOverlayPanelHiddenPos = new Vector3(0, -500, 0);
    Vector3 outroOverlayPanelHiddenPos = new Vector3(0, -1500, 0);
    //
    ControlScheme currentScheme = ControlScheme.KEYBOARDMOUSE;

    //DEV ONLY - REMOVE BEFORE BUILD
    [Header("DEV ONLY")]
    Transform cursorPositionObject;
    Transform playerPositionObject;
    public Mesh debugObjectMesh;
    public Material debugMaterial;
    [Header("Bugs / Issues")]
    [SerializeField] private List<string> knownBugs = new List<string>();

    //**FIELDS**
    public LevelEnemies LevelEnemies { get => levelEnemies; }
    public LevelDecorations LevelDecorations { get => levelDecorations; }
    public ControlScheme CurrentScheme { get => currentScheme; }
    public Vector3 CrosshairPositionIn3DSpace { get => cursorPositionObject.transform.position; }

    //**UNITY METHODS**
    private void Awake() {
        //DEV ONLY - REMOVE BEFORE BUILD - setup debug object
        cursorPositionObject = new GameObject("CursorPosObject").transform;
        cursorPositionObject.transform.parent = GameObject.FindWithTag("Player").transform;
        //cursorPositionObject = new GameObject("MousePosObject", typeof(MeshFilter), typeof(MeshRenderer)).transform;
        //cursorPositionObject.GetComponent<MeshFilter>().mesh = debugObjectMesh;
        //cursorPositionObject.GetComponent<MeshRenderer>().material = debugMaterial;

        playerPositionObject = new GameObject("PlayerPosObject").transform;
        playerPositionObject.transform.parent = GameObject.FindWithTag("Player").transform;
        //playerPositionObject = new GameObject("PlayerPosObject", typeof(MeshFilter), typeof(MeshRenderer)).transform;
        //playerPositionObject.GetComponent<MeshFilter>().mesh = debugObjectMesh;
        //playerPositionObject.GetComponent<MeshRenderer>().material = debugMaterial;

        foreach (string bug in knownBugs) {
            if (bug != "") {
                Debug.LogWarning(bug);
            }
        }

        //DEMO OVERLAY CODE

        if (playIntro) {
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

        //Enable HUD is disabled
        hud.gameObject.SetActive(true);


    }

    private void Update() {
        MoveProjectileSpawn();

        if (!outroPlayed) {
            if (playOutro) {
                outroPlayed = true;
                StartCoroutine(PlayOutroOverlay());
            }
        }

        //Determine Input Scheme
        string controlScheme = player.GetComponent<PlayerInput>().currentControlScheme;
        if (controlScheme == "Keyboard and Mouse") {
            currentScheme = ControlScheme.KEYBOARDMOUSE;
        }
        else if (controlScheme == "Gamepad") {
            currentScheme = ControlScheme.GAMEPAD;
        }
    }

    private void OnApplicationFocus(bool focus) {
        Cursor.visible = false;
    }

    //**UTILITY METHODS**
    void MoveProjectileSpawn() {

        //get mouse input position
        Vector3 screenPos = Vector3.zero;

        if (currentScheme == ControlScheme.KEYBOARDMOUSE) {
            screenPos = Mouse.current.position.ReadValue();
        }
        else if (currentScheme == ControlScheme.GAMEPAD) {

            //800x450 base canvas resolution, mult 2.4

            float canvasWidth = crosshairRect.parent.GetComponent<RectTransform>().rect.width;
            float canvasheight = crosshairRect.parent.GetComponent<RectTransform>().rect.height;

            screenPos = new Vector3((crosshairRect.anchoredPosition.x + canvasWidth / 2f) * 2.4f, (crosshairRect.anchoredPosition.y + canvasheight / 2f) * 2.4f, 0);
        }

        //Debug.Log($"Projectile Spawn screen position: {screenPos}");

        //convert mouse input to point in world 
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(Camera.main.transform.position.z)));

        //get position in center of player model
        playerPivot = new Vector3(player.position.x, player.position.y + 1.162f, 2.5f);

        //Set debug object positions
        playerPositionObject.position = playerPivot;
        cursorPositionObject.position = new Vector3(worldPos.x, worldPos.y, 2.5f);

        //setup ray 
        Ray ray = new Ray(playerPivot, (cursorPositionObject.position - playerPivot).normalized);

        //spell spawn point offset from centermass of player        
        float offset = 1.25f;//DEFAULT IS .783f ONCE SPELL COLLISION DONE

        //move projectile spawn point
        Vector3 newPoint = ray.GetPoint(offset);
        projectileSpawn.transform.position = new Vector3(newPoint.x, newPoint.y, newPoint.z);

        //DEV ONLY - REMOVE BEFORE BUILD - draw ray
        //Debug.DrawRay(centerMass, debugObject.position - centerMass, Color.red);
    }

    public Vector3 GetMousePositionInWorldSpace() {
        return cursorPositionObject.position;
    }
    //
    public Vector3 GetPlayerPivot() {
        return playerPivot;
    }
    //
    public void Quit() {
        Application.Quit();
    }
    //
    public void LoadMainMenu() {
        SceneManager.LoadScene("Splash");
    }

    //**COROUTINES**
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
    //
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

public enum ControlScheme {
    KEYBOARDMOUSE = 0,
    GAMEPAD = 1
}

