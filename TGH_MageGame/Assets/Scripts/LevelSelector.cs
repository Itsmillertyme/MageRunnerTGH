using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour {
    Button sandboxBtn;
    Button testLevelBtn;

    private void Awake() {
        sandboxBtn = GetComponentsInChildren<Button>()[0];
        testLevelBtn = GetComponentsInChildren<Button>()[1];

        sandboxBtn.onClick.AddListener(delegate { GoToScene(0); });
        testLevelBtn.onClick.AddListener(delegate { GoToScene(1); });
        SetActiveButtons();
    }

    private void SetActiveButtons() {
        //set proper buttons active
        if (SceneManager.GetActiveScene().name.Contains("Sandbox")) {
            sandboxBtn.interactable = false;
            testLevelBtn.interactable = true;
        }
        if (SceneManager.GetActiveScene().name.Contains("Level")) {
            sandboxBtn.interactable = true;
            testLevelBtn.interactable = false;
        }
    }

    public void GoToScene(int index) {
        SceneManager.LoadScene(index);
        SetActiveButtons();
    }
}
