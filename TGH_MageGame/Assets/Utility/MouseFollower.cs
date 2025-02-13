using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFollower : MonoBehaviour {

    [SerializeField] Canvas canvas;

    private void Update() {
        //get mouse input
        Vector2 mousePos = Mouse.current.position.ReadValue();

        //FULL ON WIZARDRY - Converts position on screen to a point on the canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(), mousePos, canvas.worldCamera, out Vector2 localPoint
        );

        //set anchored position
        GetComponent<RectTransform>().anchoredPosition = localPoint;
    }
}
