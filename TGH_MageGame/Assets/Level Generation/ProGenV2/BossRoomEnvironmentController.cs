using System.Collections.Generic;
using UnityEngine;

public class BossRoomEnvironmentController : MonoBehaviour {
    [SerializeField] GameObject door;
    [SerializeField] List<GameObject> platforms;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            door.SetActive(true);
            foreach (GameObject platform in platforms) {
                platform.SetActive(true);
            }
        }
    }

    public void HidePlatforms() {
        foreach (GameObject platform in platforms) {
            platform.SetActive(false);
        }
    }
}
