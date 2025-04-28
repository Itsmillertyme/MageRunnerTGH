using UnityEngine;

public class BillboardCanvas : MonoBehaviour {
    //**PROPERTIES**
    private Camera mainCamera;

    //**UNITY METHODS**
    private void Start() {
        // Cache the main camera
        mainCamera = Camera.main;
    }
    //
    private void LateUpdate() {
        if (mainCamera != null) {
            // Make the canvas face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
