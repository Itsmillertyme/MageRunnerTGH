using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] RectTransform crosshairRect;
    [SerializeField] RectTransform loadingScreen;
    [SerializeField] Image imgLoadingWheel;
    MusicManager musicManager;

    [Header("Scritable Object References")]
    [SerializeField] LevelEnemies levelEnemies;
    [SerializeField] LevelDecorations levelDecorations;
    //
    Vector3 playerPivot;
    //
    ControlScheme currentScheme = ControlScheme.KEYBOARDMOUSE;
    //
    int currentLevel = 1;

    //DEV ONLY - REMOVE BEFORE BUILD
    [Header("DEV ONLY")]
    [SerializeField] bool debugLevelGeneration;
    [SerializeField] bool debugEnemySpawning;
    [SerializeField] bool debugInput;
    [SerializeField] bool generateLevelOnLoad;
    [SerializeField] bool unlockAllPaths;
    Transform cursorPositionObject;
    Transform playerPositionObject;
    public Mesh debugObjectMesh;
    public Material debugMaterial;
    Material currentPathNodeOriginalMaterial;
    [SerializeField] Material playerRoomMaterial;
    [Header("Bugs / Issues")]
    [SerializeField] private List<string> knownBugs = new List<string>();

    //**FIELDS**
    public LevelEnemies LevelEnemies { get => levelEnemies; }
    public LevelDecorations LevelDecorations { get => levelDecorations; }
    public ControlScheme CurrentScheme { get => currentScheme; }
    public Vector3 CrosshairPositionIn3DSpace { get => cursorPositionObject.transform.position; }
    public Transform Player { get => player; }
    public bool GenerateLevelOnLoad { get => generateLevelOnLoad; set => generateLevelOnLoad = value; }
    public bool UnlockAllPaths { get => unlockAllPaths; set => unlockAllPaths = value; }
    public bool DebugLevelGeneration { get => debugLevelGeneration; set => debugLevelGeneration = value; }
    public bool DebugInput { get => debugInput; set => debugInput = value; }
    public bool DebugEnemySpawning { get => debugEnemySpawning; set => debugEnemySpawning = value; }
    public int CurrentLevel { get => currentLevel; }
    public RectTransform LoadingScreen { get => loadingScreen; set => loadingScreen = value; }

    //**UNITY METHODS**
    private void Awake() {
        //DEV ONLY - REMOVE BEFORE BUILD - setup debug object
        cursorPositionObject = new GameObject("CursorPosObject", typeof(MeshFilter), typeof(MeshRenderer)).transform;
        cursorPositionObject.transform.parent = GameObject.FindWithTag("Player").transform;
        playerPositionObject = new GameObject("PlayerPosObject", typeof(MeshFilter), typeof(MeshRenderer)).transform;
        playerPositionObject.transform.parent = GameObject.FindWithTag("Player").transform;

        if (DebugInput) {
            cursorPositionObject.GetComponent<MeshFilter>().mesh = debugObjectMesh;
            cursorPositionObject.GetComponent<MeshRenderer>().material = debugMaterial;
            playerPositionObject.GetComponent<MeshFilter>().mesh = debugObjectMesh;
            playerPositionObject.GetComponent<MeshRenderer>().material = debugMaterial;
        }

        musicManager = GameObject.Find("Music Manager").GetComponent<MusicManager>();

        //foreach (string bug in knownBugs) {
        //    if (bug != "") {
        //        Debug.LogWarning(bug);
        //    }
        //}

        //Enable HUD is disabled
        hud.gameObject.SetActive(true);

        //shenanigans        

    }
    //
    private void Update() {
        MoveProjectileSpawn();

        //Determine Input Scheme
        string controlScheme = player.GetComponent<PlayerInput>().currentControlScheme;
        if (controlScheme == "Keyboard and Mouse") {
            currentScheme = ControlScheme.KEYBOARDMOUSE;
        }
        else if (controlScheme == "Gamepad") {
            currentScheme = ControlScheme.GAMEPAD;
        }

    }
    //
    private void OnApplicationFocus(bool focus) {
        Cursor.visible = false;
    }

    //**UTILITY METHODS**
    void MoveProjectileSpawn() {

        //get mouse input position
        Vector3 screenPos = Vector3.zero;


        // Get position in center of player model
        playerPivot = new Vector3(player.position.x, player.position.y + 1.162f, 2.5f);
        // Get position of crosshair in world
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(null, crosshairRect.position);
        Vector3 worldPosition = Camera.main.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.GetComponent<Camera>().transform.position.z - 2.5f)));
        worldPosition.z = 2.5f;

        //Set debug object positions
        playerPositionObject.position = playerPivot;
        cursorPositionObject.position = new Vector3(worldPosition.x, worldPosition.y, -2.5f);

        //Setup ray 
        Ray ray = new Ray(playerPivot, (cursorPositionObject.position - playerPivot).normalized);

        //spell spawn point offset from centermass of player        
        float offset = 1.25f;//DEFAULT IS .783f ONCE SPELL COLLISION DONE

        //move projectile spawn point
        Vector3 newPoint = ray.GetPoint(offset);
        projectileSpawn.transform.position = new Vector3(newPoint.x, newPoint.y, newPoint.z);

        //DEV ONLY - REMOVE BEFORE BUILD - draw ray
        //Debug.DrawRay(centerMass, debugObject.position - centerMass, Color.red);
    }
    //
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
    public IEnumerator ShowAndHideLoadingScreen() {

        //Pause music manager
        musicManager.PauseMusic();

        //Play victory music
        GetComponent<AudioSource>().Play();

        //pause
        yield return new WaitForSeconds(5);

        imgLoadingWheel.fillAmount = 0f;
        loadingScreen.gameObject.SetActive(true);

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
        while (time < 2.9f) { //Shenanigans
            time += Time.unscaledDeltaTime;
            imgLoadingWheel.fillAmount = time / 3f;
            yield return null;
        }

        musicManager.PlayNextTrack();

        //reload level
        //GetComponent<DungeonCreator>().keepSeed = false;
        //GetComponent<DungeonCreator>().RetryGeneration();

        //Lerp small and hide
        time = 0f;
        while (time < 0.25f) {
            time += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(time / 0.25f);
            loadingScreen.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0.001f, 0.001f, 0.001f), normalizedTime);
            yield return null;
        }
        loadingScreen.gameObject.SetActive(false);
    }

}

public enum ControlScheme {
    KEYBOARDMOUSE = 0,
    GAMEPAD = 1
}