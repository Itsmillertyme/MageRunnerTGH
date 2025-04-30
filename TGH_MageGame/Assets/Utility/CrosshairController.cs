using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour {

    //**PROPERTIES**
    [Header("Settings")]
    [SerializeField] float gamePadAimSpeed;
    //
    [Header("Component References")]
    [SerializeField] Canvas canvas;
    [SerializeField] GameManager gameManager;
    //
    RectTransform rect;

    private void Awake() {
        rect = GetComponent<RectTransform>();
    }

    private void Update() {

        //Move crosshair
        if (gameManager.CurrentScheme == ControlScheme.KEYBOARDMOUSE) {
            //*Keyboard and mouse controls*
            //Debug.Log("Moving crosshair with mouse");

            //Get mouse input
            Vector2 mousePos = Mouse.current.position.ReadValue();

            //FULL ON WIZARDRY - Converts position on screen to a point on the canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), mousePos, null, out Vector2 localPoint
            );

            //Set anchored position
            GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
        else if (gameManager.CurrentScheme == ControlScheme.GAMEPAD) {
            //*Gamepad controls
            //Debug.Log("Moving crosshair with gamepad");

            // Get stick input            
            Vector2 gamepadAimInput = Gamepad.current.rightStick.ReadValue();

            //Update crosshair position
            Vector2 currentRectPosition = rect.anchoredPosition;
            currentRectPosition += (gamepadAimInput * gamePadAimSpeed);
            rect.anchoredPosition = currentRectPosition;
        }
    }
}
