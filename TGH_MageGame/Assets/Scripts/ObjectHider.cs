using UnityEngine;

public class ObjectHider : MonoBehaviour {
    [SerializeField] private GameObject tmpControlsDebug;
    private bool isActive;

    private void Start() {
        isActive = tmpControlsDebug.activeSelf;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            isActive = !isActive;
            tmpControlsDebug.SetActive(isActive);
        }
    }
}