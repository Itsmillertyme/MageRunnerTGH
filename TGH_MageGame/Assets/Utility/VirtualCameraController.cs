using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirtualCameraController : MonoBehaviour {

    //DEPRECATED - LEAVE PLAYER PREFAB IN SCENE

    public void InitializeVCams(Transform playerTrans) {
        //Initialize virtual cameras to follow and look at the player
        List<CinemachineVirtualCamera> vCams = gameObject.GetComponentsInChildren<CinemachineVirtualCamera>(true).ToList<CinemachineVirtualCamera>();
        foreach (CinemachineVirtualCamera vCam in vCams) {
            vCam.Follow = playerTrans;
            vCam.LookAt = playerTrans;
        }
    }
}
