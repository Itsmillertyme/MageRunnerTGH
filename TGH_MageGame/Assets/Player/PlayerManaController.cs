using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManaController : MonoBehaviour
{

    // This script is used to control the mana of the player
    [SerializeField]
    PlayerStats playerStats;

    // Used to check if Mana should be regenerating right now or not.
    bool regenManaReady;
    private void OnEnable()
    {
        playerStats.manaSpentEvent.AddListener(manaSpent);
    }
    private void OnDisable()
    {
        playerStats.manaSpentEvent.RemoveListener(manaSpent);
    }

    IEnumerator regenMana()
    {
        while (playerStats.getCurrentMana() < playerStats.getMaxMana())
        {
            if (!regenManaReady)
            {
                regenManaReady = true;
                yield return new WaitForSeconds(5);
            }
            else
            {
                playerStats.updateCurrentMana(1);
                yield return new WaitForSeconds(.1f);
            }
        }
        yield return null;
    }

    // If mana has been spent then stops the existing coroutine if there is one\
    // After that sets regenManaReady to false and starts the coroutine again
    void manaSpent()
    {
        StopCoroutine(regenMana());
        regenManaReady = false;
        StartCoroutine(regenMana());
    }
}
