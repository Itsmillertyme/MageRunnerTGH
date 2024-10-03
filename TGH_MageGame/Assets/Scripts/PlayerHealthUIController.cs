using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUIController : MonoBehaviour
{
    [SerializeField] PlayerHealth health;
    [SerializeField] public TextMeshProUGUI currentHealthText;
    [SerializeField] Image healthBar;

    public void UpdateUI()
    {
        Debug.Log("Updating ui");
        currentHealthText.text = "HP:\n" + health.CurrentHealth;
        healthBar.fillAmount = (float)health.CurrentHealth / health.MaxHealth;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.K))
        {
            health.RemoveFromHealth(-5);
        }
        if (Input.GetKey(KeyCode.L))
        {
            health.AddToHealth(5);
        }
    }
}
