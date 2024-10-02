using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuController : MonoBehaviour {
    [SerializeField] private CharacterController thingToMove;

    [Header("GameObject References")]
    [SerializeField] List<GameObject> locations;
    [SerializeField] Transform buttonParent;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] private GameObject debugMenu;

    bool isActive;

    private void Start() {
        isActive = debugMenu.activeSelf;

        //set up tp buttons
        for (int i = 0; i < locations.Count; i++) {
            //create a button and add to teleporter parent           
            Button button = Instantiate(buttonPrefab, buttonParent).GetComponent<Button>();
            //add listen for OnClick() to call MoveToLocation()
            int temp = i;
            button.onClick.AddListener(delegate { MoveToLocation(temp); });
            //set position
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (i * -35) - 40);
            //set text
            button.GetComponentInChildren<TextMeshProUGUI>().text = locations[i].name;
        }
    }

    private void Update() {
        // 'I' TO TOGGLE UI CANVAS
        if (Input.GetKeyDown(KeyCode.I)) {
            isActive = !isActive;
            debugMenu.SetActive(isActive);
        }
    }

    public void MoveToLocation(int index) {
        GameObject location = locations[index];

        // ONLY NECESSARY FOR CHARACTER CONTROLLER RELATED OBJECTS
        thingToMove.enabled = false;
        thingToMove.transform.position = location.transform.position;
        thingToMove.transform.rotation = location.transform.rotation;
        thingToMove.enabled = true;
    }
}