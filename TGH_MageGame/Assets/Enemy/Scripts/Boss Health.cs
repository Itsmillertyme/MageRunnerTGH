using UnityEngine;

public class BossHealth : MonoBehaviour {
    //**PROPERTIES**
    [Header("Health Base Stats")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;
    private readonly int minHealth = 0;

    [Header("Component References")]
    [SerializeField] Animator animator;

    bool isDead = false;

    //**FIELDS**
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int MinHealth => minHealth;
    public bool IsDead => isDead;

    //**UTILITY METHODS**
    public void RemoveFromHealth(int amountToRemove) {

        //remove health
        currentHealth -= amountToRemove;

        //test for death
        if (currentHealth <= minHealth) {
            currentHealth = minHealth;

            //play death animation
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                animator.CrossFade("Idle", 0f);
            }
            animator.SetTrigger("die");
            GetComponent<Collider>().enabled = false;
            isDead = true;

            //show loading screen            
            StartCoroutine(GameObject.Find("GameManager").GetComponent<GameManager>().ShowAndHideLoadingScreen());

        }
    }
}
