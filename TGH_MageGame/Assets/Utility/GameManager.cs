using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    //**PROPERTIES**
    [Header("Component References")]
    [SerializeField] Canvas hud;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] Transform player;
    [SerializeField] RectTransform crosshairRect;
    [Header("Scritable Object References")]
    [SerializeField] LevelEnemies levelEnemies;
    [SerializeField] LevelDecorations levelDecorations;
    //
    bool outroPlayed = false;
    //
    Vector3 playerPivot;
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
    public Transform Player { get => player; }

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

        //Enable HUD is disabled
        hud.gameObject.SetActive(true);


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

}

public enum ControlScheme {
    KEYBOARDMOUSE = 0,
    GAMEPAD = 1
}