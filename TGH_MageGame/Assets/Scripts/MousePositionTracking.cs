using UnityEngine;

[CreateAssetMenu]

public class MousePositionTracking : ScriptableObject
{
    [SerializeField] private Vector3 currentPosition;

    // GETTER
    public Vector3 CurrentPosition => currentPosition;

    public Vector3 GetMousePosition() {
        // Get the mouse position on the screen
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert screen position to world position
        currentPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.transform.position.z));
        return currentPosition;
    }
}