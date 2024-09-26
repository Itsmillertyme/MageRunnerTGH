using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugMenuController : MonoBehaviour
{
    [SerializeField] private CharacterController thingToMove;
    // [SerializeField] private GameObject thingToMove; // IF NOT PLAYER USE THIS INSTEAD

    [Header("GameObject References")]
    [SerializeField] private GameObject location1;
    [SerializeField] private GameObject location2;
    [SerializeField] private GameObject location3;
    [SerializeField] private GameObject location4;
    [SerializeField] private GameObject location5;

    [Header("UI References")]
    [SerializeField] private GameObject debugMenu;

    [Header("Text References")]
    [SerializeField] private Text location1Text;
    [SerializeField] private Text location2Text;
    [SerializeField] private Text location3Text;
    [SerializeField] private Text location4Text;
    [SerializeField] private Text location5Text;

    bool isActive;

    private void Start()
    {
        isActive = debugMenu.activeSelf;
        location1Text.text = location1.name;
        location2Text.text = location2.name;
        location3Text.text = location3.name;
        location4Text.text = location4.name;
        location5Text.text = location5.name;
    }

    private void Update()
    {
        // 'I' TO TOGGLE UI CANVAS
        if (Input.GetKeyDown(KeyCode.I)) {
            isActive = !isActive;
            debugMenu.SetActive(isActive);
        }
    }

    public void MoveToLocation(int index)
    {
        GameObject location = null;

        switch (index)
        {
            case 1:
                location = location1;
                break;
            case 2:
                location = location2; 
                break;
            case 3:
                location = location3;
                break;
            case 4:
                location = location4;
                break;
            case 5:
                location = location5;
                break;
        }

        // ONLY NECESSARY FOR CHARACTER CONTROLLER RELATED OBJECTS
        thingToMove.enabled = false;
        thingToMove.transform.position = location.transform.position;
        thingToMove.transform.rotation = location.transform.rotation;
        thingToMove.enabled = true;
    }
}