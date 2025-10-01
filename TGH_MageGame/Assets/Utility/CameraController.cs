using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {

    [SerializeField] List<CinemachineVirtualCamera> vCams;
    [SerializeField] TextMeshProUGUI primHUDCamTMP;
    int activeCam;
    int CAM_PRIORITY_OFFSET;

    private void Awake() {
        //Initialize Cameras              
        int priority = 0;
        CAM_PRIORITY_OFFSET = vCams.Count;
        for (int i = 0; i < vCams.Count; i++) {
            if (vCams[i].Priority > priority) {
                activeCam = i;
                priority = vCams[i].Priority;
            }
        }

        //Initialize TMP
        if (primHUDCamTMP != null) primHUDCamTMP.text = "Camera Position: " + activeCam;

    }

    private void Update() {

        //Debug.Log("Active Camera Position: " + activeCam);
    }

    public void CycleCameraPosition(InputAction.CallbackContext context) {

        //reset current cam priority
        vCams[activeCam].Priority -= CAM_PRIORITY_OFFSET;

        //cycle active cam
        activeCam = ++activeCam % vCams.Count;

        //set HUD
        primHUDCamTMP.text = "Camera Position: " + activeCam;

        //set current cam priority
        vCams[activeCam].Priority += CAM_PRIORITY_OFFSET;
    }
}
